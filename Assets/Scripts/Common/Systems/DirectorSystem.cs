using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class DirectorSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.Director;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }

            int numberToSpawn = Random.Range(3, 5);
        }
    }
}