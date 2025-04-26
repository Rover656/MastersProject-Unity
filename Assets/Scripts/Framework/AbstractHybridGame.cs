using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.EventBus;
using Rover656.Survivors.Framework.Events;
using Rover656.Survivors.Framework.Metrics;
using Rover656.Survivors.Framework.Network;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Environment = Rover656.Survivors.Framework.Systems.Environment;

namespace Rover656.Survivors.Framework {
    /// <summary>
    /// Fundamentals for a hybrid-compute game.
    /// </summary>
    public abstract class AbstractHybridGame<TGame> : IHybridGameAccess, IPacketSender where TGame : AbstractHybridGame<TGame> {
        // TODO: When connecting ensure the registries match.
        public IRegistryProvider Registries { get; }

        protected abstract Environment Environment { get; }

        private NetManager _netManager;

        protected NetManager NetManager {
            get => _netManager;
            set {
                _netManager = value;

                if (_netManager != null) {
                    _netManager.EnableStatistics = true;
                } else {
                    IsRemoteReady = false;
                }
            }
        }

        private NetPeer _netPeer;

        protected NetPeer NetPeer {
            get => _netPeer;
            set {
                _netPeer = value;
                if (_netPeer == null) {
                    IsRemoteReady = false;
                }
            }
        }
        
        protected bool IsRemoteReady { get; private set; }
        
        protected bool HasRemotePeer => NetPeer != null && (Environment == Environment.Remote || IsRemoteReady);
        
        public abstract float DeltaTime { get; }
        
        /// <summary>
        /// When handling packets, always provide the game as userdatum.
        /// </summary>
        public NetPacketProcessor NetPacketProcessor { get; } = new();
        
        private readonly Dictionary<GameSystemType, IGameSystem<TGame>> _systems = new();
        private readonly HashSet<GameSystemType> _activeSystemTypes = new();

        private readonly List<AbstractEntity> _entities = new();
        private readonly Dictionary<Guid, AbstractEntity> _entitiesById = new();

        private readonly Dictionary<ulong, Action<object>> _eventListeners = new();

        public IList<AbstractEntity> Entities => _entities;
        public Dictionary<object, List<AbstractEntity>> EntitiesByTag { get; } = new();
        public Dictionary<int, List<AbstractEntity>> EntitiesByPhysicsLayer { get; } = new();

        private bool _shouldQueueEvents;
        private readonly Queue<Action> _queuedEvents = new();
        private readonly object _queuedEventsLock = new();
        
        // Performance metrics
        private int _updatesPerSecond;
        private int _updatesThisSecond;
        private int _eventsPerSecond;
        private int _eventsThisSecond;
        private float _systemRunTimeThisSecond;
        private float _systemRuntimePerSecond;
        private float _updateTimeCounter;
        
        public readonly BasicPerformanceMonitor BasicPerformanceMonitor;
        protected bool EnablePerformanceMonitoring { get; set; }
        
        protected virtual float PerformanceTimer => Time.realtimeSinceStartup;

        protected bool IsRunning => !IsPaused && !HasQuit;
        protected bool IsPaused { get; private set; }
        protected bool HasQuit { get; private set; }

        protected bool ShouldBalanceSystems { get; set; } = true;

        protected AbstractHybridGame(IRegistryProvider registries, NetManager netManager) {
            Registries = registries;
            NetManager = netManager;

            if (!Registries.Has(FrameworkRegistries.EntityTypes)) {
                throw new ArgumentException("Entity types registry is missing!");
            }
            
            // Register nested types
            NetPacketProcessor.RegisterNestedType(
                (writer, value) => {
                    writer.Put(value.x);
                    writer.Put(value.y);
                },
                reader => {
                    var x = reader.GetFloat();
                    var y = reader.GetFloat();
                    return new Vector2(x, y);
                });
            
            // Remote setup packet
            NetPacketProcessor.SubscribeReusable<InitGameStatePacket>(Handle);
            NetPacketProcessor.SubscribeReusable<RemoteReadyPacket>(Handle);
            
            // Bulk event handler
            NetPacketProcessor.SubscribeReusable<BulkEventBundle>(HandleBulkEvents);
            
            // Register event subscriptions
            Subscribe<GameTickEvent>(Handle);
            Subscribe<GamePauseEvent>(Handle);
            Subscribe<GameResumeEvent>(Handle);
            Subscribe<GameSystemActivationEvent>(Handle, GameSystemActivationEvent.Register);
            Subscribe<EntityMovementVectorChangedEvent>(OnEntityMovementVectorChanged);
            Subscribe<EntityPositionChangedEvent>(OnEntityPositionChanged);
            Subscribe<EntitySpawnEvent>(OnEntitySpawn, EntitySpawnEvent.Register);
            Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            
            // Create performance monitor
            BasicPerformanceMonitor = new BasicPerformanceMonitor(Environment.ToString());
        }

        public void Pause() {
            Post(new GamePauseEvent());
        }

        public void Resume() {
            Post(new GameResumeEvent());
        }

        protected void Quit() {
            if (Environment != Environment.Local) {
                throw new InvalidOperationException("Cannot quit game remotely!");
            }

            // Stop the local game
            HasQuit = true;
            
            // If we're connected to a peer, disconnect
            NetPeer?.Disconnect();
            
            // Execute OnQuit actions (like showing a death screen etc.)
            OnQuit();
        }

        protected virtual void OnQuit() {
        }

        public void Send<T>(T packet, DeliveryMethod deliveryMethod, byte channel = 0) where T : class, new() {
            if (!HasRemotePeer) {
                return;
            }

            var writer = new NetDataWriter();
            NetPacketProcessor.Write(writer, packet);
            NetPeer.Send(writer, channel, deliveryMethod);
        }
        
        #region Event Bus
        
        public void Post<T>(T message) where T : AbstractEvent, new()
        {
            _eventsThisSecond++;
            
            if (_eventListeners.TryGetValue(HashCache<T>.Id, out var handler)) {
                handler(message);
            }
            
            // Send over the network, or queue if we're setting up a remote.
            lock (_queuedEventsLock) {
                if (_shouldQueueEvents) {
                    _queuedEvents.Enqueue(() => SendEventPacket(message));
                } else {
                    SendEventPacket(message);
                }
            }
        }

        public int MaxBulkPackets => 16;

        // NOTE: Packets that allow drops will drop together - this is a nasty caveat in some cases.
        public void PostMany<T>(IList<T> messages) where T : AbstractEvent, new() {
            // Arbitrary limit to keep packet size down.
            // A more mature implementation would pool to the maximum number per packet.
            if (messages.Count < 1 || messages.Count > MaxBulkPackets) {
                throw new ArgumentException($"Message count must be between 1 and {MaxBulkPackets}!");
            }
            
            var shouldBuildBundle = NetManager?.IsRunning ?? _shouldQueueEvents;

            var bundleWriter = shouldBuildBundle ? new NetDataWriter() : null;
            var bulkSender = shouldBuildBundle ? new BulkEventPacketSender(NetPacketProcessor, bundleWriter, Registries) : null;

            foreach (var message in messages) {
                _eventsThisSecond++;
            
                if (_eventListeners.TryGetValue(HashCache<T>.Id, out var handler)) {
                    handler(message);
                }

                if (!shouldBuildBundle) continue;
                
                if (message is IPacketedEvent packetedEvent) {
                    packetedEvent.SendPacket(bulkSender);
                } else {
                    NetPacketProcessor.Write(bundleWriter, message);
                }
            }

            if (shouldBuildBundle && bundleWriter.Length > 0) {
                var firstMessage = messages.First();
                
                var bundle = new BulkEventBundle {
                    EventData = bundleWriter.CopyData()
                };
                Send(bundle, firstMessage.NetworkDeliveryMethod, firstMessage.Channel);
            }
        }

        private void HandleBulkEvents(BulkEventBundle bundle) {
            NetPacketProcessor.ReadAllPackets(new NetDataReader(bundle.EventData), this);
        }

        private void SendEventPacket<T>(T message) where T : AbstractEvent, new() {
            if (message is IPacketedEvent packetedEvent) {
                packetedEvent.SendPacket(this);
            } else {
                Send(message, message.NetworkDeliveryMethod, message.Channel);
            }
        }

        protected void Subscribe<T>(Action<T> handler) where T : AbstractEvent, new() {
            Subscribe(handler, (packetProcessor, action) => packetProcessor.SubscribeReusable(action));
        }

        protected void Subscribe<T>(Action<T> handler, Action<NetPacketProcessor, Action<T>> packetRegistrar) where T : AbstractEvent {
            var id = HashCache<T>.Id;

            if (_eventListeners.ContainsKey(id)) {
                throw new InvalidOperationException($"Event listener {id} has already been subscribed!");
            }
            
            // Stored as key-value pair so we can remove listeners (later)
            _eventListeners[id] = obj => handler((T)obj);
            packetRegistrar(NetPacketProcessor, packet => {
                _eventsThisSecond++;
                handler(packet);
            });
        }

        /// <summary>
        /// Use this to collect network messages when there isn't a remote peer.
        /// This is used to fire any missed events once the initial game state is established remotely.
        /// </summary>
        protected void BeginNetworkEventQueue() {
            lock (_queuedEventsLock) {
                if (_shouldQueueEvents) {
                    return;
                }
            
                _shouldQueueEvents = true;
            }
        }

        /// <summary>
        /// Finishes queueing events and fires them all over the network to a remote peer (if present)
        /// </summary>
        protected void EndNetworkEventQueue() {
            lock (_queuedEventsLock) {
                while (_queuedEvents.Count > 0) {
                    var queuedEvent = _queuedEvents.Dequeue();
                    queuedEvent();
                }

                _queuedEvents.Clear();
                _shouldQueueEvents = false;
            }
        }
        
        #endregion
        
        #region Spawn Remote Game

        protected void SerializeWorld(NetDataWriter writer) {
            // Write paused state
            writer.Put(IsPaused);
            
            // Write all active remote services into the packet. (Mainly for the use of the benchmark setting)
            var inactiveSystemTypes = Registries.Get(FrameworkRegistries.GameSystemTypes).Entries
                .Where(e => !_activeSystemTypes.Contains(e))
                .ToList();
            
            writer.Put(inactiveSystemTypes.Count);
            foreach (var systemType in inactiveSystemTypes) {
                writer.Put(Registries.GetIdFrom(FrameworkRegistries.GameSystemTypes, systemType));
            }
            
            // Write all entities into the packet.
            writer.Put(_entities.Count);
            foreach (var entity in _entities) {
                writer.Put(Registries.GetIdFrom(FrameworkRegistries.EntityTypes, entity.Type));
                entity.Serialize(writer);
            }
            
            SerializeAdditional(writer);
        }

        private void Handle(InitGameStatePacket initState) {
            if (Environment != Environment.Remote) {
                throw new InvalidOperationException();
            }
            
            Debug.Log("Received initial game state from the local game.");
            
            DeserializeWorld(new NetDataReader(initState.RawData));
            
            var writer = new NetDataWriter();
            NetPacketProcessor.Write(writer, new RemoteReadyPacket());
            NetPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Debug.Log("Sending ready signal to receive events and systems.");
        }

        private void Handle(RemoteReadyPacket remoteReadyPacket) {
            if (Environment != Environment.Local) {
                throw new InvalidOperationException();
            }
            
            Debug.Log("Remote is ready, firing queued events.");
            
            // Finish queueing messages for the remote and fire them all.
            EndNetworkEventQueue();
            
            // Trigger an update immediately.
            NetManager?.TriggerUpdate();
            
            // Start sending packets direct to the remote.
            IsRemoteReady = true;
        }

        protected void DeserializeWorld(NetDataReader reader) {
            // Load pause state
            IsPaused = reader.GetBool();
            
            // Load all active remote systems.
            var systemCount = reader.GetInt();
            for (var i = 0; i < systemCount; i++) {
                var systemTypeId = reader.GetInt();
                var systemType = Registries.GetFrom(FrameworkRegistries.GameSystemTypes, systemTypeId);
                _activeSystemTypes.Add(systemType);
            }
            
            // Spawn all entities from the remote.
            var entityCount = reader.GetInt();
            for (var i = 0; i < entityCount; i++) {
                var entityTypeId = reader.GetInt();
                var entityType = Registries.GetFrom(FrameworkRegistries.EntityTypes, entityTypeId);
                OnEntitySpawn(new EntitySpawnEvent() {
                    Entity = entityType.FromNetwork(reader),
                });
            }
            
            DeserializeAdditional(reader);
        }
        
        protected virtual void SerializeAdditional(NetDataWriter writer) {}
        protected virtual void DeserializeAdditional(NetDataReader reader) {}
        
        #endregion
        
        #region Systems & Schedulling

        private void TryOffloadSystem()
        {
            if (!HasRemotePeer)
            {
                return;
            }
            
            var mostImpactfulSystemType = _activeSystemTypes
                .Where(e => e.EnvironmentConstraint != EnvironmentConstraint.LocalOnly)
                .OrderBy(x => x.EnvironmentConstraint)
                .ThenByDescending(x => x.ImpactScore)
                .FirstOrDefault();

            if (mostImpactfulSystemType != null)
            {
                var name = Registries.GetNameFrom(FrameworkRegistries.GameSystemTypes, mostImpactfulSystemType);
                Debug.Log($"Game system {name} has been offloaded.");
                
                SetSystemEnvironment(mostImpactfulSystemType, Environment.Remote);
            }
        }

        private void TryOnloadSystem()
        {
            var leastImpactfulSystemType = Registries.Get(FrameworkRegistries.GameSystemTypes).Entries
                .Where(e => !_activeSystemTypes.Contains(e))
                .Where(e => e.EnvironmentConstraint != EnvironmentConstraint.LocalOnly)
                .OrderByDescending(x => x.EnvironmentConstraint)
                .ThenBy(x => x.ImpactScore)
                .FirstOrDefault();

            if (leastImpactfulSystemType != null)
            {
                var name = Registries.GetNameFrom(FrameworkRegistries.GameSystemTypes, leastImpactfulSystemType);
                Debug.Log($"Game system {name} has been onloaded.");
                
                SetSystemEnvironment(leastImpactfulSystemType, Environment.Local);
            }
        }

        protected void ForceOnloadAll()
        {
            foreach (var type in Registries.Get(FrameworkRegistries.GameSystemTypes).Entries)
            {
                // Don't go breaking anything :)
                if (type.EnvironmentConstraint == EnvironmentConstraint.LocalOnly) {
                    continue;
                }
                
                SetSystemEnvironment(type, Environment.Local);
            }
        }

        protected void ForceOffloadAll()
        {
            foreach (var type in Registries.Get(FrameworkRegistries.GameSystemTypes).Entries)
            {
                SetSystemEnvironment(type, Environment.Remote);
            }
        }

        protected THybridSystem AddSystem<THybridSystem>(THybridSystem system)
            where THybridSystem : IGameSystem<TGame> {
            if (!_systems.TryAdd(system.Type, system))
            {
                throw new ArgumentException("A system is already added for this system type.");
            }

            // Systems always active on client to start with.
            SetSystemEnvironment(system.Type, Environment.Local);
            
            return system;
        }

        protected void SetSystemEnvironment(GameSystemType type, Environment targetEnvironment)
        {
            if (type.EnvironmentConstraint == EnvironmentConstraint.LocalOnly && targetEnvironment != Environment.Local)
            {
                throw new InvalidOperationException("Cannot move LocalOnly system to the Remote!");
            }
            
            Post(new GameSystemActivationEvent
            {
                Type = type,
                ActiveEnvironment = targetEnvironment,
            });
        }
        
        #endregion

        protected AbstractEntity GetEntity(Guid entityId) {
            return _entitiesById.GetValueOrDefault(entityId);
        }

        protected T AddNewEntity<T>(T entity) where T : AbstractEntity
        {
            return AddNewEntity(entity, Vector2.zero);
        }

        public T AddNewEntity<T>(T entity, Vector2 position) where T : AbstractEntity
        {
            entity.Position = position;
            Post(new EntitySpawnEvent {
                Entity = entity,
            });
            return entity;
        }

        public void DestroyEntity(Guid entityId)
        {
            Post(new EntityDestroyedEvent
            {
                EntityId = entityId,
            });
        }

        public bool HasTag(AbstractEntity entity, object tag)
        {
            return EntitiesByTag.TryGetValue(tag, out var entities) &&
                entities.Contains(entity);
        }

        public virtual void Update() {
            // Performance timing
            _updateTimeCounter += Time.deltaTime;
            _updatesThisSecond++;

            if (_updateTimeCounter > 1.0f)
            {
                if (EnablePerformanceMonitoring) {
                    BasicPerformanceMonitor.Report(PerformanceTimer, _entities.Count, _updatesThisSecond, _eventsThisSecond, 
                        _activeSystemTypes.Count, _systemRunTimeThisSecond, NetPeer?.Ping ?? 0, NetManager?.Statistics);
                }
                
                // Debug.Log($"Updates this second were: {_updatesThisSecond}");
                
                // Local makes decisions on system load
                if (ShouldBalanceSystems && Environment == Environment.Local)
                {
                    if (_systemRunTimeThisSecond > 0.1f || _updatesThisSecond < Application.targetFrameRate / 2)
                    {
                        TryOffloadSystem();
                    }
                    else if (_systemRunTimeThisSecond < 0.1f && _updatesThisSecond >= Application.targetFrameRate / 4 * 3)
                    {
                        TryOnloadSystem();
                    }
                }
                
                // Computes average.
                _updatesPerSecond = Mathf.FloorToInt((_updatesPerSecond + _updatesThisSecond) / (1 + _updateTimeCounter));
                _eventsPerSecond = Mathf.FloorToInt((_eventsPerSecond + _eventsThisSecond) / (1 + _updateTimeCounter));
                _systemRuntimePerSecond = Mathf.FloorToInt((_systemRuntimePerSecond + _systemRunTimeThisSecond) / (1 + _updateTimeCounter));
                _updateTimeCounter = 0;
                _updatesThisSecond = 0;
                _eventsThisSecond = 0;
                _systemRunTimeThisSecond = 0;

                // Debug.Log($"Updates per second: {_updatesPerSecond}");
                // Debug.Log($"Events per second: {_eventsPerSecond}");
            }
            
            if (Environment == Environment.Local) {
                // Fire game tick.
                var meta = new NetDataWriter();
                SerializeTickMeta(meta);
                Post(new GameTickEvent
                {
                    MetaData = meta.Data,
                });
            }

            // Do not run systems while paused.
            if (!IsRunning) {
                return;
            }
            
            // Update all systems.
            var systemStartTime = Time.realtimeSinceStartup;
            foreach (var systemType in _activeSystemTypes)
            {
                if (_systems.TryGetValue(systemType, out var system))
                {
                    system.Update((TGame)this, DeltaTime);
                }
            }

            var systemEndTime = Time.realtimeSinceStartup;
            var systemRunTime = systemEndTime - systemStartTime;

            _systemRunTimeThisSecond += systemRunTime;
            
            // Debug.Log($"System execution time: {systemEndTime - systemStartTime} secs");
        }
        
        // region Game tick metadata

        protected virtual void SerializeTickMeta(NetDataWriter writer)
        {
        }

        protected virtual void DeserializeTickMeta(NetDataReader reader)
        {
        }
        
        // endregion

        #region Network handlers

        private void Handle(GameTickEvent tickEvent)
        {
            // Only the remote receives tick metadata.
            if (Environment == Environment.Remote)
            {
                DeserializeTickMeta(new NetDataReader(tickEvent.MetaData));
            }
        }

        private void Handle(GameSystemActivationEvent systemActivationEvent)
        {
            if (systemActivationEvent.ActiveEnvironment == Environment)
            {
                _activeSystemTypes.Add(systemActivationEvent.Type);
            }
            else
            {
                _activeSystemTypes.Remove(systemActivationEvent.Type);
            }
        }

        protected virtual void OnEntitySpawn(EntitySpawnEvent spawnEvent) {
            // Debug.Log($"Spawning entity {spawnEvent.Entity.Id} on {Environment}");
            
            _entities.Add(spawnEvent.Entity);
            _entitiesById.Add(spawnEvent.Entity.Id, spawnEvent.Entity);

            foreach (var tag in spawnEvent.Entity.Type.Tags)
            {
                if (!EntitiesByTag.TryGetValue(tag, out var tagList))
                {
                    tagList = new List<AbstractEntity>();
                    EntitiesByTag.Add(tag, tagList);
                }
                
                tagList.Add(spawnEvent.Entity);
            }

            if (!EntitiesByPhysicsLayer.TryGetValue(spawnEvent.Entity.PhysicsLayer, out var collisionList))
            {
                collisionList = new();
                EntitiesByPhysicsLayer.Add(spawnEvent.Entity.PhysicsLayer, collisionList);
            }
            collisionList.Add(spawnEvent.Entity);
            
            spawnEvent.Entity.Game = this;
        }

        private void OnEntityMovementVectorChanged(EntityMovementVectorChangedEvent changedEvent) {
            if (!_entitiesById.TryGetValue(changedEvent.EntityId, out var entity)) {
                Debug.LogWarning($"Received movement vector change for non-existent entity {changedEvent.EntityId} on {Environment}.");
                return;
            }
            
            OnEntityMovementVectorChanged(entity, changedEvent.MovementVector);
        }

        protected virtual void OnEntityMovementVectorChanged(AbstractEntity entity, Vector2 movementVector) {
            entity.MovementVector = movementVector;
        }

        private void OnEntityPositionChanged(EntityPositionChangedEvent changedEvent) {
            if (!_entitiesById.TryGetValue(changedEvent.EntityId, out var entity)) {
                Debug.LogWarning($"Received position change for non-existent entity {changedEvent.EntityId} on {Environment}.");
                return;
            }
            
            OnEntityPositionChanged(entity, changedEvent.Position);
        }

        protected virtual void OnEntityPositionChanged(AbstractEntity entity, Vector2 position) {
            entity.Position = position;
        }

        protected void OnEntityDestroyed(EntityDestroyedEvent destroyedEvent) {
            if (!_entitiesById.TryGetValue(destroyedEvent.EntityId, out var entity))
            {
                return;
            }
            
            OnEntityDestroyed(entity);
            _entities.Remove(entity);
            _entitiesById.Remove(entity.Id);
            
            foreach (var tag in entity.Type.Tags)
            {
                if (EntitiesByTag.TryGetValue(tag, out var tagList))
                {
                    tagList.Remove(entity);
                }
            }

            if (EntitiesByPhysicsLayer.TryGetValue(entity.PhysicsLayer, out var list))
            {
                list.Remove(entity);
            }
            
            entity.Game = null;
        }

        protected virtual void OnEntityDestroyed(AbstractEntity entity) {
        }

        private void Handle(GamePauseEvent gamePauseEvent) {
            IsPaused = true;
            OnPaused();
        }

        protected virtual void OnPaused() {
        }

        private void Handle(GameResumeEvent resumeEvent) {
            IsPaused = false;
            OnResumed();
        }

        protected virtual void OnResumed() {
        }

        #endregion
        
        // Code borrowed from NetPacketProcessor
        private static class HashCache<T>
        {
            public static readonly ulong Id;

            //FNV-1 64 bit hash
            static HashCache()
            {
                var hash = 14695981039346656037UL; //offset
                var typeName = typeof(T).ToString();
                foreach (var character in typeName) {
                    hash ^= character;
                    hash *= 1099511628211UL; //prime
                }
                Id = hash;
            }
        }
    }
}