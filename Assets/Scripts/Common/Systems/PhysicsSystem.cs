using System;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Utility;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems
{
    /// <summary>
    /// The physics system handles entity movement & collision.
    /// This will not use Unity's systems for collision as the entities are not necessarily stored in a scene.
    /// </summary>
    public class PhysicsSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.Physics;

        // public void Update(AbstractLevel abstractLevel, float deltaTime)
        // {
        //     // TODO: Add latency to delta time...
        //     
        //     // Apply movements
        //     foreach (var entity in abstractLevel.Entities)
        //     {
        //         // TODO: Collision checks.
        //         if (entity.Velocity.magnitude > 0)
        //         {
        //             entity.SetPosition(entity.Position + (entity.Velocity * deltaTime));
        //         }
        //     }
        // }

        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            foreach (var layer in abstractLevel.EntitiesByPhysicsLayer.Keys)
            {
                foreach (var entity in abstractLevel.EntitiesByPhysicsLayer[layer])
                {
                    if (entity.Velocity.magnitude <= 0)
                    {
                        continue;
                    }
                    
                    Vector2 velocity = entity.Velocity;
                    Vector2 proposedPosition = entity.Position + (velocity * deltaTime);

                    // Check for potential collisions with other entities
                    if (entity.CanCollide)
                    {
                        foreach (var other in abstractLevel.EntitiesByPhysicsLayer[layer])
                        {
                            if (entity == other || !other.CanCollide) continue; // Don't check self-collision

                            if (proposedPosition.Intersects(entity.Size, other.Bounds, out var penetrationVector)) {
                                // Separate entities if they are fully intersecting
                                entity.SetPosition(entity.Position + penetrationVector);

                                // Stop movement if the entity collides in its path
                                velocity = Vector2.zero;
                                break; // Stop checking further if a collision occurs
                            }
                        }
                    }

                    // Update position if no collision happened
                    entity.SetPosition(entity.Position + (velocity * deltaTime));
                }
            }
        }
    }
}