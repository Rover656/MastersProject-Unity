using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Common.Systems.EnemyMovement;
using Rover656.Survivors.Framework;
using UnityEngine;
using Environment = Rover656.Survivors.Framework.Systems.Environment;
using ParticleSystem = Rover656.Survivors.Common.Systems.ParticleSystem;

namespace Rover656.Survivors.Common.World {
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel> {
        public virtual float GameTime { get; protected set; }
        
        private float _deltaTime;

        public override float DeltaTime => _deltaTime;

        protected override float PerformanceTimer => GameTime;

        public Player Player { get; protected set; }

        // Client only property
        public int? MaxPlayTime { get; }

        private LevelMode _levelMode;

        public LevelMode LevelMode {
            get => _levelMode;
            private set {
                _levelMode = value;

                ShouldBalanceSystems = _levelMode is not (LevelMode.LocalBenchmark or LevelMode.RemoteBenchmark);
                EnablePerformanceMonitoring = _levelMode is not LevelMode.StandardPlay;
            }
        }

        protected AbstractLevel(NetManager netManager, LevelMode levelMode = LevelMode.StandardPlay, int? maxPlayTime = null) : base(SurvivorsRegistries.Instance, netManager) {
            LevelMode = levelMode;
            MaxPlayTime = maxPlayTime;
            
            // Register all systems.
            AddSystem(new PhysicsSystem());
            AddSystem(new DumbFollowerSystem());
            AddSystem(new DistancedFollowerSystem());
            AddSystem(new DamageSystem());
            AddSystem(new WeaponSystem());
            AddSystem(new ParticleSystem());
            AddSystem(new DirectorSystem());
            AddSystem(new ExperienceSystem());

            if (LevelMode != LevelMode.StandardPlay) {
                // Arbitrary load to pretend the game is more computationally expensive than it actually is.
                AddSystem(new ArbitraryLoadSystem());
            }

            // Force all systems to the remote server immediately. Balancing is off so they'll remain remote.
            if (LevelMode == LevelMode.RemoteBenchmark) {
                ForceOffloadAll();
            }

            // Subscribe to game events
            Subscribe<EntityHealthChangedEvent>(OnEntityHealthChanged, EntityHealthChangedEvent.Register);
            Subscribe<EntityDiedEvent>(OnEntityDied);
            Subscribe<PlayerCollectItemEvent>(OnPlayerCollectedItem, PlayerCollectItemEvent.Register);
            Subscribe<PlayerExperienceEvent>(OnPlayerExperienceChanged);
            Subscribe<PlayerLevelUpEvent>(OnPlayerLevelChanged);
        }

        public override void Update() {
            if (HasQuit) {
                return;
            }
            
            _deltaTime = Time.deltaTime;
            
            // Client handles time advancement.
            if (Environment == Environment.Local && !IsPaused)
            {
                GameTime += DeltaTime;

                if (GameTime > MaxPlayTime) {
                    Quit();
                    return;
                }
            }

            base.Update();
        }

        public bool EveryNSeconds(float seconds, float offset = 0)
        {
            /*return Mathf.FloorToInt(GameTime) !=
                   Mathf.FloorToInt(GameTime - DeltaTime) &&
                   Mathf.Approximately(Mathf.FloorToInt(GameTime) % seconds, Mathf.Epsilon);*/

            if (seconds <= 0) {
                throw new ArgumentException("Seconds must be greater than 0");
            }

            if (seconds < DeltaTime) {
                return true;
            }

            if (GameTime < seconds) {
                return false;
            }

            float prevTime = (GameTime + offset) - DeltaTime;

            // Check if we crossed a multiple of `seconds`
            return Mathf.FloorToInt((GameTime + offset) / seconds) != Mathf.FloorToInt(prevTime / seconds);
        }
        
        protected override void SerializeTickMeta(NetDataWriter writer)
        {
            base.SerializeTickMeta(writer);
            writer.Put(GameTime);
        }

        protected override void DeserializeTickMeta(NetDataReader reader)
        {
            base.DeserializeTickMeta(reader);
            float newGameTime = reader.GetFloat();
            // _deltaTime = newGameTime - GameTime;
            GameTime = newGameTime;
        }

        protected override void SerializeAdditional(NetDataWriter writer) {
            base.SerializeAdditional(writer);
            
            // Save player Guid for during reconstruction
            writer.Put(Player.Id);
            
            // TODO: How do we sync the time once the remote is established??
            // Maybe the client should send a game time heartbeat?
            writer.Put(GameTime);

            // Stupid way of sending, but works for demo.
            writer.Put(LevelMode.ToString());
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            
            var playerId = reader.GetGuid();
            Player = (Player)GetEntity(playerId);
            
            GameTime = reader.GetFloat();

            string levelModeString = reader.GetString();
            if (!Enum.TryParse<LevelMode>(levelModeString, out var levelMode)) {
                throw new Exception($"Invalid level mode: {levelModeString}");
            }

            LevelMode = levelMode;
        }

        protected virtual void OnEntityHealthChanged(EntityHealthChangedEvent healthChangedEvent) {
            if (GetEntity(healthChangedEvent.EntityId) is not IDamageable damageable) return;

            // Handle immediately to prevent god spam
            if (healthChangedEvent.InvincibleUntil.HasValue) {
                damageable.LocalSetInvincibleUntil(healthChangedEvent.InvincibleUntil.Value);
            }

            // Player is in "God" mode when we're benchmarking.
            if (LevelMode != LevelMode.StandardPlay && healthChangedEvent.EntityId == Player.Id) {
                return;
            }

            // Debug.Log($"Entity {healthChangedEvent.EntityId} took {healthChangedEvent.Delta} damage.");

            damageable.LocalSetHealth(damageable.Health - healthChangedEvent.Delta);

            // Client has authority over death
            if (Environment == Environment.Local) {
                if (damageable.Health <= 0) {
                    Post(new EntityDiedEvent {
                        EntityId = healthChangedEvent.EntityId,
                    });
                }
            }
        }

        private float ExperienceThreshold => LevelMode == LevelMode.StandardPlay ? 0.2f : 0.3f;

        protected virtual void OnEntityDied(EntityDiedEvent diedEvent) {
            // Client has authority over death actions
            if (Environment == Environment.Local) {
                if (Player.Id == diedEvent.EntityId) {
                    // Quit the game if the player dies and display the death screen.
                    Quit();
                } else {
                    // Spawn experience shards
                    var entity = GetEntity(diedEvent.EntityId);
                    if (entity is Enemy enemy) {
                        if (enemy.GetOffset(0f, 1f) < ExperienceThreshold) {
                            AddNewEntity(EntityTypes.BasicExperienceShard.Create(), enemy.Position);
                        }
                    }
                
                    DestroyEntity(diedEvent.EntityId);
                }
            }
        }

        protected virtual void OnPlayerCollectedItem(PlayerCollectItemEvent collectEvent) {
            Player.LocalAddItem(collectEvent.Stack);
        }

        protected virtual void OnPlayerExperienceChanged(PlayerExperienceEvent experienceEvent) {
            Player.Experience = experienceEvent.Experience;
        }
        
        protected virtual void OnPlayerLevelChanged(PlayerLevelUpEvent levelEvent) {
            if (levelEvent.Level != Player.Level) {
                Player.UpdateStats();
                
                // Local set is fine here because event is fired on both sides.
                // TODO: In paper discuss that sometimes an event is unnecessary, but at the cost of possible desync.
                Player.LocalSetHealth(Player.MaxHealth);
            }
            
            Player.Level = levelEvent.Level;
            Player.NextExperienceMilestone = levelEvent.NextExperienceMilestone;
        }
    }
}