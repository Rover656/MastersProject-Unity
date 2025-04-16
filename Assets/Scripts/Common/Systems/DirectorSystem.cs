using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class DirectorSystem : IGameSystem<AbstractLevel>
    {
        public GameSystemType Type => SystemTypes.Director;

        private const float SpawnWidth = 20;
        private const float SpawnHeight = 15;
        
        // Doesn't matter that this is lost between remote and client that much.
        private readonly System.Random _random = new();

        private readonly EntityType<Enemy>[] _enemyTypes = {
            EntityTypes.Bat, EntityTypes.RuneWizard, 
        };
        
        private readonly List<EnemyInfo> _enemyInfo = new()
        {
            // TODO _00 is temp
            new(EntityTypes.Bat, 1, 5_00, 10, 3),
            new(EntityTypes.RuneWizard, 4, 15_00, 1, 12),
        };
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }
            
            int stageLevel = 1 + Mathf.FloorToInt(abstractLevel.GameTime / 30);

            // Inflate stage level for benchmarks
            if (abstractLevel.LevelMode != LevelMode.StandardPlay) {
                stageLevel += 20;
            }
            
            float spawnRate = Mathf.Max(2f - stageLevel * 0.2f, 0.75f);

            if (abstractLevel.EveryNSeconds(spawnRate))
            {
                int credits = Mathf.RoundToInt(5f + 3f * (stageLevel - 1));

                // Spend credits to spawn enemies
                while (credits > 0) {
                    // Attempt to pick an enemy
                    var enemy = PickEnemy(stageLevel, credits);
                    if (enemy is null) {
                        break;
                    }
                    
                    // Spawn the enemy and deduct its cost
                    abstractLevel.AddNewEntity(enemy.EntityType.Create(), GetEnemySpawnPosition(player.Position));
                    credits -= enemy.Cost;
                }
            }
        }
        
        [CanBeNull]
        private EnemyInfo PickEnemy(int level, int remainingBalance)
        {
            var availableEnemies = _enemyInfo.Where(e => level >= e.MinLevel && level <= e.MaxLevel && e.Cost <= remainingBalance).ToList();
            if (availableEnemies.Count == 0)
                return null;

            int totalWeight = availableEnemies.Sum(e => e.Weight);
            int roll = _random.Next(totalWeight);
            int sum = 0;

            foreach (var enemy in availableEnemies)
            {
                sum += enemy.Weight;
                if (roll < sum)
                    return enemy;
            }
            return null;
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

        private class EnemyInfo {
            public EntityType<Enemy> EntityType { get; }
            public int MinLevel { get; }
            public int MaxLevel { get; }
            public int Weight { get; }
            public int Cost { get; }
            
            public EnemyInfo(EntityType<Enemy> entityType, int minLevel, int maxLevel, int weight, int cost)
            {
                EntityType = entityType;
                MinLevel = minLevel;
                MaxLevel = maxLevel;
                Weight = weight;
                Cost = cost;
            }
        }
    }
}