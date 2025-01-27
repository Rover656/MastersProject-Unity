using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Network;
using UnityEngine;

namespace Rover656.Survivors.Framework {
    /// <summary>
    /// Fundamentals for a hybrid-compute game.
    /// </summary>
    public abstract class AbstractHybridGame<TGame> where TGame : AbstractHybridGame<TGame> {
        public abstract SystemEnvironment SystemEnvironment { get; }
        public abstract float NetworkDelay { get; }

        protected NetManager NetManager { get; }
        protected NetPeer NetPeer { get; set; }
        protected NetPacketProcessor NetPacketProcessor { get; } = new();

        private readonly List<IHybridSystem<TGame>> _systems = new();

        private readonly List<AbstractEntity<TGame>> _entities = new();
        private readonly Dictionary<Guid, AbstractEntity<TGame>> _entitiesById = new();

        public IEnumerable<AbstractEntity<TGame>> Entities => _entities;

        protected AbstractHybridGame(NetManager netManager) {
            NetManager = netManager;

            // Register packet handlers.
            // TODO: This packet cannot be serialized currently.
            //NetPacketProcessor.SubscribeReusable<SpawnEntityPacket<TGame>>(Handle);
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

        public AbstractEntity<TGame> GetEntity(Guid entityId) {
            return _entitiesById[entityId];
        }

        public T AddNewEntity<T>(T entity) where T : AbstractEntity<TGame> {
            _entities.Add(entity);
            _entitiesById.Add(entity.Id, entity);
            OnEntityAdded(entity);

            // Send to remote, if connected.
            Send(new SpawnEntityPacket<TGame>() {
                Entity = entity,
            }, DeliveryMethod.ReliableOrdered);

            return entity;
        }

        public void Update() {
            // Update all systems.
            foreach (var system in _systems) {
                system.Update((TGame)this, Time.deltaTime);
            }
        }

        public virtual void OnEntityAdded(AbstractEntity<TGame> entity) {
            entity.Game = (TGame)this;
        }

        public virtual void OnEntityMoved(AbstractEntity<TGame> entity) {
        }

        #region Network handlers

        private void Handle(SpawnEntityPacket<TGame> spawnEntityPacket) {
            _entities.Add(spawnEntityPacket.Entity);
            _entitiesById.Add(spawnEntityPacket.Entity.Id, spawnEntityPacket.Entity);
            OnEntityAdded(spawnEntityPacket.Entity);
        }

        private void Handle(EntityPositionUpdatePacket entityPositionUpdatePacket) {
            // TODO: Better way to post entity movements -or- a different event strategy for the Unity link.
            var entity = GetEntity(entityPositionUpdatePacket.EntityId);
            entity.Position = entity.Position;
            OnEntityMoved(entity);
        }

        #endregion
    }
}