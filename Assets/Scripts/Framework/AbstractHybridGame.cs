using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Network;
using UnityEngine;

namespace Rover656.Survivors.Framework {
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
        protected NetPacketProcessor NetPacketProcessor { get; } = new();

        private readonly List<IHybridSystem<TGame>> _systems = new();

        private readonly List<AbstractEntity> _entities = new();
        private readonly Dictionary<Guid, AbstractEntity> _entitiesById = new();

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
            
            // Register packet handlers.
            NetPacketProcessor.SubscribeReusable<SpawnEntityPacket>(Handle);
            NetPacketProcessor.SubscribeReusable<EntityPositionUpdatePacket>(Handle);
            NetPacketProcessor.SubscribeReusable<DestroyEntityPacket>(Handle);
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

        protected THybridSystem AddSystem<THybridSystem>(THybridSystem system)
            where THybridSystem : IHybridSystem<TGame> {
            _systems.Add(system);
            return system;
        }

        public AbstractEntity GetEntity(Guid entityId) {
            return _entitiesById[entityId];
        }

        public T AddNewEntity<T>(T entity) where T : AbstractEntity {
            _entities.Add(entity);
            _entitiesById.Add(entity.Id, entity);
            OnEntityAdded(entity);

            // Send to remote, if connected.
            Send(new SpawnEntityPacket(entity), DeliveryMethod.ReliableOrdered);

            return entity;
        }

        public void Update() {
            // Update all systems.
            foreach (var system in _systems) {
                system.Update((TGame)this, Time.deltaTime);
            }
        }

        public virtual void OnEntityAdded(AbstractEntity entity) {
            entity.Game = (TGame)this;
        }

        public virtual void OnEntityMoved(AbstractEntity entity) {
        }

        public virtual void OnEntityRemoved(AbstractEntity entity) {
        }

        #region Network handlers

        private void Handle(SpawnEntityPacket spawnEntityPacket) {
            var entity = spawnEntityPacket.CreateEntityFrom(this);
            
            _entities.Add(entity);
            _entitiesById.Add(entity.Id, entity);
            OnEntityAdded(entity);
        }

        private void Handle(EntityPositionUpdatePacket entityPositionUpdatePacket) {
            var entity = GetEntity(entityPositionUpdatePacket.EntityId);
            entity.Position = entity.Position;
            OnEntityMoved(entity);
        }

        private void Handle(DestroyEntityPacket destroyEntityPacket) {
            if (_entitiesById.Remove(destroyEntityPacket.EntityId, out var entity)) {
                _entities.Remove(entity);
                OnEntityRemoved(entity);
            }
        }

        #endregion
    }
}