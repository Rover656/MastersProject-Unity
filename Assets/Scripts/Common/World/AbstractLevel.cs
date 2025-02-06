using System;
using LiteNetLib;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Common.Systems.EnemyMovement;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.World {
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel> {
        public float GameTime { get; set; }

        public Player Player { get; }

        public PhysicsSystem PhysicsSystem { get; }
        public DumbFollowerSystem DumbFollowerSystem { get; }
        public DamageSystem DamageSystem { get; }

        protected AbstractLevel(NetManager netManager) : base(SurvivorsRegistries.Instance, netManager) {
            // Register all systems.
            PhysicsSystem = AddSystem(new PhysicsSystem());
            DumbFollowerSystem = AddSystem(new DumbFollowerSystem());
            DamageSystem = AddSystem(new DamageSystem());

            // Subscribe to game events
            SubscribeNetSerializable<EntityHealthChangedEvent>(OnEntityHealthChanged);
            Subscribe<EntityDiedEvent>(OnEntityDied);
            Subscribe<PlayerCollectItemEvent>(OnPlayerCollectedItem, PlayerCollectItemEvent.Register);

            // Spawn the player
            Player = AddNewEntity(EntityTypes.Player.Create());

            // Add an example enemy (will be the job of the director system soon)
            AddNewEntity(EntityTypes.Bat.Create(), new Vector2(1, 2));
            AddNewEntity(EntityTypes.Bat.Create(), new Vector2(2, 1));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(1, 1));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(0, 2));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(2, 0));
        }

        public override void Update() {
            GameTime += Time.deltaTime;
            base.Update();
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