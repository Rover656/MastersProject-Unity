using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Events;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems.EnemyMovement
{
    public class DistancedFollowerSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.DistancedFollower;

        private const int DistanceToMaintain = 4;
        private const int DistanceToMelee = 1;

        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            // Only run 15 times per second, we don't need hyper fast follower systems
            if (!abstractLevel.EveryNSeconds(1 / 15f)) {
                return;
            }
            
            if (!abstractLevel.EntitiesByTag.TryGetValue(EnemyMovementTag.DistancedFollower, out var enemies))
            {
                return;
            }

            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }
            
            var vectorChanges = new List<(Guid, Vector2)>();
            foreach (var enemy in enemies)
            {
                var toPlayer = player.Position - enemy.Position;

                Vector2 vector;
                if (toPlayer.magnitude < DistanceToMelee) {
                    vector = toPlayer.normalized;
                } else if (toPlayer.magnitude < DistanceToMaintain) {
                    vector = -toPlayer.normalized;
                } else if (toPlayer.magnitude > DistanceToMaintain * 1.2f) {
                    vector = toPlayer.normalized;
                } else {
                    vector = Vector2.zero;
                }

                if (enemy.MovementVector != vector) {
                    vectorChanges.Add((enemy.Id, vector));
                }
            }
            
            // Bulk fire events
            var movementChangeEvents = new List<EntityMovementVectorChangedEvent>(abstractLevel.MaxBulkPackets);
            for (var i = 0; i < vectorChanges.Count; i += abstractLevel.MaxBulkPackets) {
                // Flush for next batch
                movementChangeEvents.Clear();
                
                // Add all events
                for (var j = i; j < vectorChanges.Count && j < i + abstractLevel.MaxBulkPackets; j++) {
                    var change = vectorChanges[j];
                    movementChangeEvents.Add(new EntityMovementVectorChangedEvent
                    {
                        EntityId = change.Item1,
                        MovementVector = change.Item2,
                    });
                }
                
                // Fire all events
                abstractLevel.PostMany(movementChangeEvents);
            }
        }
    }
}