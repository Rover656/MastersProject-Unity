using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Utility;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Events;
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

        private const float UpdateRate = 1 / 60f;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime) {
            if (!abstractLevel.EveryNSeconds(UpdateRate)) {
                return;
            }

            // If delta time is larger, we're lagging so let the system compensate.
            if (deltaTime < UpdateRate) {
                deltaTime = UpdateRate;
            }
            
            // Fake load to try and cause a performance impact.
            // counter exists to ensure this loop has a side effect
            // ulong counter = 0;
            // for (ulong i = 0; i < 50_000_000; i++)
            // {
            //     counter += i;
            // }

            var positionChanges = new List<(Guid, Vector2)>();
            
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
                                //entity.SetPosition(entity.Position + penetrationVector);
                                
                                var newPosition = entity.Position + penetrationVector;
                                if (entity.Position != newPosition) {
                                    positionChanges.Add((entity.Id, newPosition));
                                }

                                // Stop movement if the entity collides in its path
                                velocity = Vector2.zero;
                                break; // Stop checking further if a collision occurs
                            }
                        }
                    }

                    // Update position if no collision happened
                    if (velocity.magnitude > 0) {
                        var newPosition = entity.Position + (velocity * deltaTime);
                        if (entity.Position != newPosition) {
                            positionChanges.Add((entity.Id, newPosition));
                        }
                    }
                }
            }
            
            // Bulk fire events
            var positionChangeEvents = new List<EntityPositionChangedEvent>(abstractLevel.MaxBulkPackets);
            for (var i = 0; i < positionChanges.Count; i += abstractLevel.MaxBulkPackets) {
                // Flush for next batch
                positionChangeEvents.Clear();
                
                // Add all events
                for (var j = i; j < positionChanges.Count && j < i + abstractLevel.MaxBulkPackets; j++) {
                    var change = positionChanges[j];
                    positionChangeEvents.Add(new EntityPositionChangedEvent()
                    {
                        EntityId = change.Item1,
                        Position = change.Item2,
                    });
                }
                
                // Fire all events
                abstractLevel.PostMany(positionChangeEvents);
            }
        }
    }
}