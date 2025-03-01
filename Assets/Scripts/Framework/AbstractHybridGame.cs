using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.EventBus;
using Rover656.Survivors.Framework.Events;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;
using Environment = Rover656.Survivors.Framework.Systems.Environment;

namespace Rover656.Survivors.Framework {
    // TODO: Methods to serialize the entire game state for a large initialization packet.
    /// <summary>
    /// Fundamentals for a hybrid-compute game.
    /// </summary>
    public abstract class AbstractHybridGame<TGame> : IHybridGameAccess where TGame : AbstractHybridGame<TGame> {
        public abstract SystemEnvironment SystemEnvironment { get; }
        
        // TODO: When connecting ensure the registries match.
        public IRegistryProvider Registries { get; }

        public abstract Environment Environment { get; }
        
        protected NetManager NetManager { get; set; }
        protected NetPeer NetPeer { get; set; }
        
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

        public IEnumerable<AbstractEntity> Entities => _entities;
        public Dictionary<object, List<AbstractEntity>> EntitiesByTag { get; } = new();
        public Dictionary<int, List<AbstractEntity>> EntitiesByPhysicsLayer { get; } = new();

        private bool _shouldQueueEvents = false;
        private Queue<Action> _queuedEvents = new();
        private object _queuedEventsLock = new();
        
        // Performance metrics
        private int _updatesPerSecond;
        private int _updatesThisSecond;
        private float _updateTimeCounter;

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
                (reader) => {
                    var x = reader.GetFloat();
                    var y = reader.GetFloat();
                    return new Vector2(x, y);
                });
            
            // Register event subscriptions
            Subscribe<GameTickEvent>(Handle);
            Subscribe<GameSystemActivationEvent>(Handle, GameSystemActivationEvent.Register);
            Subscribe<EntityMovementVectorChangedEvent>(OnEntityMovementVectorChanged);
            Subscribe<EntityPositionChangedEvent>(OnEntityPositionChanged);
            Subscribe<EntitySpawnEvent>(OnEntitySpawn, EntitySpawnEvent.Register);
            Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        public void Send<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new() {
            if (NetManager == null || NetPeer == null) {
                return;
            }

            var writer = new NetDataWriter();
            NetPacketProcessor.Write(writer, packet);
            NetPeer.Send(writer, deliveryMethod);
        }
        
        #region Event Bus

        public void Post<T>(T message) where T : AbstractEvent, new() {
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
        
        private void SendEventPacket<T>(T message) where T : AbstractEvent, new() {
            if (message is IPacketedEvent packetedEvent) {
                packetedEvent.SendPacket(this);
            } else {
                Send(message, message.NetworkDeliveryMethod);                        
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
            packetRegistrar(NetPacketProcessor, handler);
        }

        /// <summary>
        /// Use this to collect network messages when there isn't a remote peer.
        /// This is used to fire any missed events once the initial game state is established remotely.
        /// </summary>
        protected void BeginNetworkEventQueue() {
            if (_shouldQueueEvents) {
                return;
            }
            
            _shouldQueueEvents = true;
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
        
        private void ConnectToRemoteServer() {
            // Immediately begin collecting any new events to update the remote state once it is established.
            BeginNetworkEventQueue();
        }

        public void SerializeWorld(NetDataWriter writer) {
            // Write all entities into the packet.
            writer.Put(_entities.Count);
            for (int i = 0; i < _entities.Count; i++) {
                writer.Put(Registries.GetIdFrom(FrameworkRegistries.EntityTypes, _entities[i].Type));
                _entities[i].Serialize(writer);
            }
            
            SerializeAdditional(writer);
        }

        public void DeserializeWorld(NetDataReader reader) {
            // Spawn all entities from the remote.
            int entityCount = reader.GetInt();
            for (int i = 0; i < entityCount; i++) {
                int entityTypeId = reader.GetInt();
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
            var mostImpactfulSystemType = _activeSystemTypes
                .Where(e => e.EnvironmentConstraint != EnvironmentConstraint.LocalOnly)
                .OrderBy(x => x)
                .FirstOrDefault();

            if (mostImpactfulSystemType != null)
            {
                string name = Registries.GetNameFrom(FrameworkRegistries.GameSystemTypes, mostImpactfulSystemType);
                Debug.Log($"Game system {name} has been offloaded.");
                
                SetSystemEnvironment(mostImpactfulSystemType, Environment.Remote);
            }
        }

        private void TryOnloadSystem()
        {
            var leastImpactfulSystemType = Registries.Get(FrameworkRegistries.GameSystemTypes).Entries
                .Where(e => !_activeSystemTypes.Contains(e))
                .Where(e => e.EnvironmentConstraint != EnvironmentConstraint.LocalOnly)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (leastImpactfulSystemType != null)
            {
                SetSystemEnvironment(leastImpactfulSystemType, Environment.Local);
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

        public AbstractEntity GetEntity(Guid entityId) {
            return _entitiesById[entityId];
        }

        public T AddNewEntity<T>(T entity) where T : AbstractEntity
        {
            return AddNewEntity(entity, Vector2.zero);
        }

        public T AddNewEntity<T>(T entity, Vector2 position) where T : AbstractEntity
        {
            entity.Position = position;
            Post(new EntitySpawnEvent() {
                Entity = entity,
            });
            return entity;
        }

        public void DestroyEntity(Guid entityId)
        {
            Post(new EntityDestroyedEvent()
            {
                EntityId = entityId,
            });
        }

        public virtual void Update() {
            // Performance timing
            _updateTimeCounter += Time.deltaTime;
            _updatesThisSecond++;

            if (_updateTimeCounter > 1.0f)
            {
                // Debug.Log($"Updates this second were: {_updatesThisSecond}");
                
                // Computes average.
                _updatesPerSecond = Mathf.FloorToInt((_updatesPerSecond + _updatesThisSecond) / (1 + _updateTimeCounter));
                _updateTimeCounter = 0;
                _updatesThisSecond = 0;
                
                // Debug.Log($"Updates per second updated to: {_updatesPerSecond}");
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
            
            // Update all systems.
            float systemStartTime = Time.realtimeSinceStartup;
            foreach (var systemType in _activeSystemTypes)
            {
                if (_systems.TryGetValue(systemType, out var system))
                {
                    system.Update((TGame)this, DeltaTime);
                }
            }

            float systemEndTime = Time.realtimeSinceStartup;
            float systemRunTime = systemEndTime - systemStartTime;

            // Local makes decisions on system load
            if (Environment == Environment.Local)
            {
                if (systemRunTime > 0.0001f)
                {
                    TryOffloadSystem();
                }
                else if (systemRunTime < 0.0000001f)
                {
                    TryOnloadSystem();
                }
            }
            
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

        protected void Handle(GameTickEvent tickEvent)
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
            _entities.Add(spawnEvent.Entity);
            _entitiesById.Add(spawnEvent.Entity.Id, spawnEvent.Entity);

            foreach (var tag in spawnEvent.Entity.Type.Tags)
            {
                if (!EntitiesByTag.TryGetValue(tag, out var tagList))
                {
                    tagList = new();
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

        protected void OnEntityMovementVectorChanged(EntityMovementVectorChangedEvent changedEvent) {
            if (!_entitiesById.TryGetValue(changedEvent.EntityId, out var entity)) {
                // Warn instead and some kind of recovery?
                throw new InvalidOperationException("Entity does not exist!");
            }
            
            entity.MovementVector = changedEvent.MovementVector;
        }

        protected void OnEntityPositionChanged(EntityPositionChangedEvent changedEvent) {
            if (!_entitiesById.TryGetValue(changedEvent.EntityId, out var entity)) {
                // Warn instead and some kind of recovery?
                throw new InvalidOperationException("Entity does not exist!");
            }
            
            OnEntityPositionChanged(entity, changedEvent.Position);
        }

        // TODO: Could be another sub-packet?
        protected virtual void OnEntityPositionChanged(AbstractEntity entity, Vector2 position) {
            entity.Position = position;
        }

        protected void OnEntityDestroyed(EntityDestroyedEvent destroyedEvent) {
            if (!_entitiesById.TryGetValue(destroyedEvent.EntityId, out var entity)) {
                // Warn instead and some kind of recovery?
                throw new InvalidOperationException("Entity does not exist!");
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

        #endregion
        
        // Code borrowed from NetPacketProcessor
        private static class HashCache<T>
        {
            public static readonly ulong Id;

            //FNV-1 64 bit hash
            static HashCache()
            {
                ulong hash = 14695981039346656037UL; //offset
                string typeName = typeof(T).ToString();
                for (var i = 0; i < typeName.Length; i++)
                {
                    hash ^= typeName[i];
                    hash *= 1099511628211UL; //prime
                }
                Id = hash;
            }
        }
    }
}