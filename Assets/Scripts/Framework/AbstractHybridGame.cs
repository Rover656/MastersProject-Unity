using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.EventBus;
using Rover656.Survivors.Framework.Events;
using UnityEngine;

namespace Rover656.Survivors.Framework {
    // TODO: Methods to serialize the entire game state for a large initialization packet.
    /// <summary>
    /// Fundamentals for a hybrid-compute game.
    /// </summary>
    public abstract class AbstractHybridGame<TGame> : IHybridGameAccess where TGame : AbstractHybridGame<TGame> {
        public abstract SystemEnvironment SystemEnvironment { get; }
        public abstract float NetworkDelay { get; }
        
        // TODO: When connecting ensure the registries match.
        public IRegistryProvider Registries { get; }

        protected NetManager NetManager { get; }
        protected NetPeer NetPeer { get; set; }
        
        /// <summary>
        /// When handling packets, always provide the game as userdatum.
        /// </summary>
        protected NetPacketProcessor NetPacketProcessor { get; } = new();

        private readonly List<IHybridSystem<TGame>> _systems = new();

        private readonly List<AbstractEntity> _entities = new();
        private readonly Dictionary<Guid, AbstractEntity> _entitiesById = new();

        private readonly Dictionary<ulong, Action<object>> _eventListeners = new();

        public IEnumerable<AbstractEntity> Entities => _entities;

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
                (reader) => new Vector2(reader.GetFloat(), reader.GetFloat()));
            
            // Register event subscriptions
            Subscribe<EntityPositionChangedEvent>(OnEntityPositionChanged);
            Subscribe<EntitySpawnEvent>(OnEntitySpawn, EntitySpawnEvent.Register);
            Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        public void Send<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new() {
            // TODO: Should we call the handle method for the provided packet too? Make all logic centralised around these messages?

            if (NetManager == null || NetPeer == null) {
                return;
            }

            var writer = new NetDataWriter();
            NetPacketProcessor.Write(writer, packet);
            NetPeer.Send(writer, deliveryMethod);
        }
        
        #region Event Bus

        public void Post<T>(T message) where T : AbstractEvent {
            if (_eventListeners.TryGetValue(HashCache<T>.Id, out var handler)) {
                handler(message);
            }
            
            // Send over the network
            Send(message.GetForNetwork(), message.NetworkDeliveryMethod);
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
        
        #endregion

        protected THybridSystem AddSystem<THybridSystem>(THybridSystem system)
            where THybridSystem : IHybridSystem<TGame> {
            _systems.Add(system);
            return system;
        }

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

        public void Update() {
            // Update all systems.
            foreach (var system in _systems) {
                system.Update((TGame)this, Time.deltaTime);
            }
        }

        #region Network handlers

        protected virtual void OnEntitySpawn(EntitySpawnEvent spawnEvent) {
            _entities.Add(spawnEvent.Entity);
            _entitiesById.Add(spawnEvent.Entity.Id, spawnEvent.Entity);
            spawnEvent.Entity.Game = this;
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