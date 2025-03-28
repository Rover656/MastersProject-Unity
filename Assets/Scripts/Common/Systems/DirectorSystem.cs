using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class DirectorSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.Director;

        private const float SpawnWidth = 20;
        private const float SpawnHeight = 15;

        private readonly EntityType<Enemy>[] _enemyTypes = {
            EntityTypes.Bat, EntityTypes.RuneWizard, 
        };
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }

            if (abstractLevel.EveryNSeconds(0.5f))
            {
                int numberToSpawn = Random.Range(1, 5);

                for (int i = 0; i < numberToSpawn; i++)
                {
                    var enemyType = _enemyTypes[Random.Range(0, _enemyTypes.Length)];
                    abstractLevel.AddNewEntity(enemyType.Create(), GetEnemySpawnPosition(player.Position));                    
                }
            }
        }

        private Vector2 GetEnemySpawnPosition(Vector2 playerPosition)
        {
            int edge = Random.Range(0, 4); // 0 = top, 1 = bottom, 2 = left, 3 = right
            float randomX = 0f;
            float randomY = 0f;

            switch (edge)
            {
                case 0: // Top edge
                    randomX = Random.Range(-SpawnWidth / 2, SpawnWidth / 2);
                    randomY = SpawnHeight / 2;
                    break;
                case 1: // Bottom edge
                    randomX = Random.Range(-SpawnWidth / 2, SpawnWidth / 2);
                    randomY = -SpawnHeight / 2;
                    break;
                case 2: // Left edge
                    randomX = -SpawnWidth / 2;
                    randomY = Random.Range(-SpawnHeight / 2, SpawnHeight / 2);
                    break;
                case 3: // Right edge
                    randomX = SpawnWidth / 2;
                    randomY = Random.Range(-SpawnHeight / 2, SpawnHeight / 2);
                    break;
            }

            return playerPosition + new Vector2(randomX, randomY);
        }
    }
}