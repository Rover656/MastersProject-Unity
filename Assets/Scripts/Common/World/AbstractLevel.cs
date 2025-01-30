using System;
using LiteNetLib;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.World
{
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel>
    {
        public int GameTime { get; set; }

        public Player Player { get; }

        public PhysicsSystem PhysicsSystem { get; }
        
        protected AbstractLevel(NetManager netManager) : base(SurvivorsRegistries.Instance, netManager)
        {
            // Register all systems.
            PhysicsSystem = AddSystem(new PhysicsSystem());

            // Subscribe to game events
            Subscribe<EntityDamageEvent>(OnEntityDamaged);
            Subscribe<EntityHealEvent>(OnEntityHealed);
            
            // Spawn the player
            Player = AddNewEntity(EntityTypes.Player.Create());
            
            // Add an example enemy (will be the job of the director system soon)
            AddNewEntity(EntityTypes.Bat.Create(), new Vector2(2, 2));
        }

        protected virtual void OnEntityDamaged(EntityDamageEvent damageEvent)
        {
            if (GetEntity(damageEvent.EntityId) is not IDamageable damageable) return;
            
            damageable.LocalSetHealth(damageable.Health - damageEvent.Damage);

            if (damageable.Health <= 0)
            {
                DestroyEntity(damageEvent.EntityId);
            }
        }

        protected virtual void OnEntityHealed(EntityHealEvent healEvent)
        {
            if (GetEntity(healEvent.EntityId) is not IDamageable damageable) return;
            
            damageable.LocalSetHealth(Math.Min(damageable.Health + healEvent.Healing, damageable.MaxHealth));
        }
    }
}