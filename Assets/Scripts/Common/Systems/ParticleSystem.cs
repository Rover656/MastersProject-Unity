using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rover656.Survivors.Common.Systems {
    public class ParticleSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.Particle;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            // Get all particles
            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Particle, out var particles)) {
                return;
            }
            
            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Enemy, out var enemies)) {
                return;
            }
            
            var particlesToRemove = new List<Guid>();
            
            foreach (var particle in particles)
            {
                if (particle is WeaponParticle weaponParticle)
                {
                    if (abstractLevel.GameTime > weaponParticle.AliveUntil)
                    {
                        particlesToRemove.Add(weaponParticle.Id);
                        continue;
                    }
                    
                    // TODO: handle particle movements

                    if (weaponParticle.MovementType == ParticleMovementType.AimRandomTarget &&
                        weaponParticle.MovementVector.magnitude <= Mathf.Epsilon)
                    {
                        if (weaponParticle.IsPlayerParticle) {
                            if (enemies.Count > 0)
                            {
                                var enemy = enemies[Random.Range(0, enemies.Count)];
                                weaponParticle.SetMovementVector((enemy.Position - weaponParticle.Position).normalized);
                            }
                            else
                            {
                                weaponParticle.SetMovementVector(Random.insideUnitCircle.normalized);
                            }
                        } else {
                            weaponParticle.SetMovementVector((abstractLevel.Player.Position - weaponParticle.Position).normalized);
                        }
                    }

                    if (weaponParticle.MovementType == ParticleMovementType.RandomDirection &&
                        weaponParticle.MovementVector.magnitude <= Mathf.Epsilon)
                    {
                        weaponParticle.SetMovementVector(Random.insideUnitCircle.normalized);
                    }
                }
            }
            
            foreach (var particleId in particlesToRemove)
            {
                abstractLevel.DestroyEntity(particleId);
            }
        }
    }
}