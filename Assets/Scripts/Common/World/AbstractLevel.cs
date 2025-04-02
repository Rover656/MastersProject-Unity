using System;
using System.Linq;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Common.Systems.EnemyMovement;
using Rover656.Survivors.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Environment = Rover656.Survivors.Framework.Systems.Environment;
using ParticleSystem = Rover656.Survivors.Common.Systems.ParticleSystem;

namespace Rover656.Survivors.Common.World {
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel> {
        public virtual float GameTime { get; protected set; }

        private float _deltaTime;

        public override float DeltaTime => _deltaTime;

        public Player Player { get; protected set; }

        protected AbstractLevel(NetManager netManager) : base(SurvivorsRegistries.Instance, netManager) {
            // Register all systems.
            AddSystem(new PhysicsSystem());
            AddSystem(new DumbFollowerSystem());
            AddSystem(new DistancedFollowerSystem());
            AddSystem(new DamageSystem());
            AddSystem(new WeaponSystem());
            AddSystem(new ParticleSystem());
            AddSystem(new DirectorSystem());
            AddSystem(new ExperienceSystem());

            // Subscribe to game events
            Subscribe<EntityHealthChangedEvent>(OnEntityHealthChanged, EntityHealthChangedEvent.Register);
            Subscribe<EntityDiedEvent>(OnEntityDied);
            Subscribe<PlayerCollectItemEvent>(OnPlayerCollectedItem, PlayerCollectItemEvent.Register);
            Subscribe<PlayerExperienceEvent>(OnPlayerExperienceChanged);
            Subscribe<PlayerLevelUpEvent>(OnPlayerLevelChanged);
        }

        public override void Update() {
            _deltaTime = Time.deltaTime;
            
            // Client handles time advancement.
            if (Environment == Environment.Local && !IsPaused)
            {
                GameTime += DeltaTime;
            }

            base.Update();

            if (EveryNSeconds(15)) {
                BasicPerformanceMonitor.SaveToFile();
                
                Debug.Log($"Entity count: {Entities.Count}");
            }
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
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            
            var playerId = reader.GetGuid();
            Player = (Player)GetEntity(playerId);
            
            GameTime = reader.GetFloat();
        }

        protected virtual void OnEntityHealthChanged(EntityHealthChangedEvent healthChangedEvent) {
            if (GetEntity(healthChangedEvent.EntityId) is not IDamageable damageable) return;

            // Debug.Log($"Entity {healthChangedEvent.EntityId} took {healthChangedEvent.Delta} damage.");

            damageable.LocalSetHealth(damageable.Health - healthChangedEvent.Delta);

            if (healthChangedEvent.InvincibleUntil.HasValue) {
                damageable.LocalSetInvincibleUntil(healthChangedEvent.InvincibleUntil.Value);
            }

            if (damageable.Health <= 0) {
                Post(new EntityDiedEvent {
                    EntityId = healthChangedEvent.EntityId,
                });
            }
        }

        protected virtual void OnEntityDied(EntityDiedEvent diedEvent) {
            if (Player.Id == diedEvent.EntityId) {
                // Debug.Log("Player died! Need to pause the game loop and show death screen etc (i.e. hand back to Unity)");
            } else {
                // Spawn experience shards
                var entity = GetEntity(diedEvent.EntityId);
                if (entity is Enemy enemy) {
                    if (enemy.GetOffset(0f, 1f) < 0.2f) {
                        AddNewEntity(EntityTypes.BasicExperienceShard.Create(), enemy.Position);
                    }
                }
                
                DestroyEntity(diedEvent.EntityId);
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