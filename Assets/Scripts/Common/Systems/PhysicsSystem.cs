using System;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems
{
    /// <summary>
    /// The physics system handles entity movement & collision.
    /// This will not use Unity's systems for collision as the entities are not necessarily stored in a scene.
    /// </summary>
    public class PhysicsSystem : IHybridSystem<AbstractLevel>
    {
        public int SystemId => 1;

        public bool IsActive { get; set; }

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

                            if (IsColliding(proposedPosition, entity.Size, other.Position, other.Size, out Vector2 penetrationVector))
                            {
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

        // TODO: Testing ChatGPT solution to collisions
        bool IsColliding(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB, out Vector2 penetrationVector)
        {
            float leftA = posA.x;
            float rightA = posA.x + sizeA.x;
            float topA = posA.y;
            float bottomA = posA.y + sizeA.y;

            float leftB = posB.x;
            float rightB = posB.x + sizeB.x;
            float topB = posB.y;
            float bottomB = posB.y + sizeB.y;

            if (rightA > leftB && leftA < rightB && bottomA > topB && topA < bottomB)
            {
                // Calculate penetration vector to push entities apart
                float overlapX = Math.Min(rightA - leftB, rightB - leftA);
                float overlapY = Math.Min(bottomA - topB, bottomB - topA);

                if (overlapX < overlapY)
                {
                    penetrationVector = new Vector2(overlapX * (posA.x < posB.x ? -1 : 1), 0);
                }
                else
                {
                    penetrationVector = new Vector2(0, overlapY * (posA.y < posB.y ? -1 : 1));
                }

                return true;
            }

            penetrationVector = Vector2.zero;
            return false;
        }
    }
}