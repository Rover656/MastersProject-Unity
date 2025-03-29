using System.Linq;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
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
            // Only run 20 times per second, we don't need hyper fast follower systems
            if (!abstractLevel.EveryNSeconds(1 / 20f)) {
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
            
            foreach (var enemy in enemies)
            {
                var toPlayer = (player.Position - enemy.Position);

                if (toPlayer.magnitude < DistanceToMelee) {
                    enemy.SetMovementVector(toPlayer.normalized);
                } else if (toPlayer.magnitude < DistanceToMaintain) {
                    enemy.SetMovementVector(-toPlayer.normalized);
                } else if (toPlayer.magnitude > DistanceToMaintain * 1.2f) {
                    enemy.SetMovementVector(toPlayer.normalized);
                } else {
                    enemy.SetMovementVector(Vector2.zero);
                }
            }
        }
    }
}