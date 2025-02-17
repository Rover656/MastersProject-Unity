using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Utility;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Common.Systems {
    public class DamageSystem : IGameSystem<AbstractLevel> {
        public GameSystemType Type => SystemTypes.Damage;

        // TODO: Maybe its worth having these tick at a fixed rate and handling that in the game.
        // That way when we account for network latency we can possibly run additional ticks to catch up?
        // Unsure, but currently if we passed latency into the delta it'd not "speed" up this system.
        public void Update(AbstractLevel abstractLevel, float deltaTime) {
            // Get all damageable and damager entities
            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Damageable, out var damageables)) {
                return;
            }

            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Damageable, out var damagers)) {
                return;
            }

            // Collect all damage and post after it is all collected
            // Prevents collection mutation during iteration
            Dictionary<Guid, (int, float)> pendingDamage = new();

            // Note this could be optimised, but I think it might be okay given we want to show a difference between local & remote compute?
            foreach (var damageSourceEntity in damagers) {
                if (damageSourceEntity is not IDamageSource damageSource) {
                    continue;
                }

                foreach (var damageableEntity in damageables) {
                    if (damageableEntity is not IDamageable damageable) {
                        continue;
                    }

                    // Ensure damage appears to this group.
                    if (damageSource.DamagesPhysicsLayer != damageableEntity.PhysicsLayer) {
                        continue;
                    }
                    
                    // Do not apply damage if we have a pending invincibility frame.
                    // This is done in case an entity is purposely given no i-frames.
                    if (pendingDamage.TryGetValue(damageableEntity.Id, out var pendingDamagePair) && pendingDamagePair.Item2 > abstractLevel.GameTime) {
                        continue;
                    }

                    // Do not apply any more damage to an invincible entity.
                    if (damageable.InvincibleUntil > abstractLevel.GameTime) {
                        continue;
                    }

                    // If there is a collision, deal the contact damage.
                    if (damageSourceEntity.Bounds.Intersects(damageableEntity.Bounds, out _)) {
                        var damageDealt = damageable.CalculateDamageTaken(damageSource.Damage);
                        if (damageDealt > 0) {
                            pendingDamage.Add(damageableEntity.Id, (damageDealt, abstractLevel.GameTime + damageable.InvincibilityDuration));
                        }
                    }
                }
            }

            // Post all the damage for this tick.
            foreach (var pair in pendingDamage) {
                abstractLevel.Post(new EntityHealthChangedEvent {
                    EntityId = pair.Key,
                    Delta = pair.Value.Item1,
                    InvincibleUntil = pair.Value.Item2,
                });
            }
        }
    }
}