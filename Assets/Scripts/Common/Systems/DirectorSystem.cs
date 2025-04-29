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

        private const float SpawnWidth = 25;
        private const float SpawnHeight = 20;

        private const int BenchMaxLevel = 5_000_000;
        private const int MaxLevel = 5 * 60 / 20; // based on 5 mins to win.
        
        // Doesn't matter that this is lost between remote and client that much.
        private readonly System.Random _random = new();
        
        private readonly List<EnemyInfo> _benchmarkEnemyInfo = new()
        {
            new(EntityTypes.Bat, 1, 25, 18, 3),
            new(EntityTypes.RuneWizard, 4, 25, 4, 12),
            new(EntityTypes.Ghost, 7, 30, 12, 8),
            new(EntityTypes.VileGhost, 10, BenchMaxLevel, 7, 12),
            new(EntityTypes.Spider, 5, BenchMaxLevel, 5, 30),
            new(EntityTypes.ElderWizard, 7, BenchMaxLevel, 2, 25),
        };
        
        private readonly List<EnemyInfo> _enemyInfo = new()
        {
            new(EntityTypes.Bat, 1, 8, 18, 3),
            new(EntityTypes.RuneWizard, 4, 10, 4, 12),
            new(EntityTypes.Ghost, 7, 10, 12, 8),
            new(EntityTypes.VileGhost, 9, MaxLevel, 7, 12),
            new(EntityTypes.Spider, 5, MaxLevel, 5, 30),
            new(EntityTypes.ElderWizard, 7, MaxLevel, 2, 25),
        };
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }

            // Some settings were changed since benchmark results were taken.
            // To keep it level, the director behaves differently in standard play.
            // This was done to improve "fun" during demo.
            var isBenchmark = abstractLevel.LevelMode != LevelMode.StandardPlay;
            
            float stageLevelIncreaseRate = isBenchmark ? 30 : 20;
            var stageLevel = 1 + Mathf.FloorToInt(abstractLevel.GameTime / stageLevelIncreaseRate);

            // Inflate stage level for benchmarks
            var enemyDifficultyModifier = 1;
            if (!isBenchmark) {
                // Only increase enemy stats in standard play, cap at 10x difficulty
                enemyDifficultyModifier = Mathf.Min(10, Mathf.FloorToInt(Mathf.Exp(0.1f * stageLevel)));
            } else {
                stageLevel += 20 + abstractLevel.Player.Level;
            }

            stageLevel = Mathf.Max(0, stageLevel);
            
            var spawnRate = Mathf.Max(2f - stageLevel * 0.2f, isBenchmark ? 0.75f : 0.3f);
            if (abstractLevel.EveryNSeconds(spawnRate))
            {
                var credits = Mathf.RoundToInt(5f + 3f * (stageLevel - 1));

                // Spend credits to spawn enemies
                while (credits > 0) {
                    // Attempt to pick an enemy
                    var enemy = PickEnemy(stageLevel, credits, isBenchmark);
                    if (enemy is null) {
                        break;
                    }
                    
                    // Spawn the enemy and deduct its cost
                    var enemyEntity = enemy.EntityType.Create();
                    enemyEntity.DifficultyMultiplier = enemyDifficultyModifier;
                    abstractLevel.AddNewEntity(enemyEntity, GetEnemySpawnPosition(player.Position));
                    credits -= enemy.Cost;
                }
            }
        }
        
        // PickEnemy and GetEnemySpawnPosition developed and adapted with assistance from generative AI.
        [CanBeNull]
        private EnemyInfo PickEnemy(int level, int remainingBalance, bool isBenchmark)
        {
            var availableEnemies = (isBenchmark ? _benchmarkEnemyInfo : _enemyInfo)
                .Where(e => level >= e.MinLevel && level <= e.MaxLevel && e.Cost <= remainingBalance).ToList();
            if (availableEnemies.Count == 0)
                return null;

            var totalWeight = availableEnemies.Sum(e => e.Weight);
            var roll = _random.Next(totalWeight);
            var sum = 0;

            foreach (var enemy in availableEnemies)
            {
                sum += enemy.Weight;
                if (roll < sum)
                    return enemy;
            }
            return null;
        }

        private static Vector2 GetEnemySpawnPosition(Vector2 playerPosition)
        {
            var edge = Random.Range(0, 4); // 0 = top, 1 = bottom, 2 = left, 3 = right
            var randomX = 0f;
            var randomY = 0f;

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