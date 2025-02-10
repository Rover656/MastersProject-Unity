using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Common.Systems.EnemyMovement;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.World {
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel> {
        public float GameTime { get; private set; }

        public Player Player { get; protected set; }

        public PhysicsSystem PhysicsSystem { get; }
        public DumbFollowerSystem DumbFollowerSystem { get; }
        public DamageSystem DamageSystem { get; }

        protected AbstractLevel(NetManager netManager) : base(SurvivorsRegistries.Instance, netManager) {
            // Register all systems.
            PhysicsSystem = AddSystem(new PhysicsSystem());
            DumbFollowerSystem = AddSystem(new DumbFollowerSystem());
            DamageSystem = AddSystem(new DamageSystem());

            // Subscribe to game events
            Subscribe<EntityHealthChangedEvent>(OnEntityHealthChanged, EntityHealthChangedEvent.Register);
            Subscribe<EntityDiedEvent>(OnEntityDied);
            Subscribe<PlayerCollectItemEvent>(OnPlayerCollectedItem, PlayerCollectItemEvent.Register);
        }

        public override void Update() {
            GameTime += Time.deltaTime;
            base.Update();
        }

        protected override void SerializeAdditional(NetDataWriter writer) {
            base.SerializeAdditional(writer);
            
            // Save player Guid for during reconstruction
            writer.Put(Player.Id);
            
            // TODO: How do we sync the time once the remote is established??
            // Maybe the client should send a game time heartbeat?
            writer.Put(GameTime);
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            
            var playerId = reader.GetGuid();
            Player = (Player)GetEntity(playerId);
            
            GameTime = reader.GetFloat();
        }

        protected virtual void OnEntityHealthChanged(EntityHealthChangedEvent healthChangedEvent) {
            if (GetEntity(healthChangedEvent.EntityId) is not IDamageable damageable) return;

            Debug.Log($"Entity {healthChangedEvent.EntityId} took {healthChangedEvent.Delta} damage.");

            damageable.LocalSetHealth(damageable.Health - healthChangedEvent.Delta);

            if (healthChangedEvent.InvincibleUntil.HasValue) {
                damageable.LocalSetInvincibleUntil(healthChangedEvent.InvincibleUntil.Value);
            }

            if (damageable.Health <= 0) {
                Post(new EntityDiedEvent {
                    EntityId = healthChangedEvent.EntityId,
                });
            }
        }

        protected virtual void OnEntityDied(EntityDiedEvent diedEvent) {
            if (Player.Id == diedEvent.EntityId) {
                Debug.Log("Player died! Need to pause the game loop and show death screen etc (i.e. hand back to Unity)");
            } else {
                DestroyEntity(diedEvent.EntityId);
            }
        }

        protected virtual void OnPlayerCollectedItem(PlayerCollectItemEvent collectEvent) {
            Player.LocalAddItem(collectEvent.Stack);
        }
    }
}