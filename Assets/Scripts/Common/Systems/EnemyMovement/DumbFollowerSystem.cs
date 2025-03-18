using System.Linq;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Common.Systems.EnemyMovement
{
    public class DumbFollowerSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.DumbFollower;

        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            if (!abstractLevel.EntitiesByTag.TryGetValue(EnemyMovementTag.DumbFollower, out var enemies))
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
                var movementVector = (player.Position - enemy.Position).normalized;
                enemy.SetMovementVector(movementVector);
            }
        }
    }
}