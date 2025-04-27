using System.Collections.Generic;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class ExperienceSystem : IGameSystem<AbstractLevel> {
        public GameSystemType Type => SystemTypes.Experience;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime) {
            // TODO: Find XP objects the player is close to, collect them then fire events
            // if the XP threshold is met, also trigger a level up event.
            
            var player = abstractLevel.Player;
            
            // Get all experience objects
            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Experience, out var experienceShards)) {
                return;
            }
            
            // Find any experience shards that are close to the player
            List<ExperienceShard> collected = new();
            foreach (var xp in experienceShards) {
                if (xp is not ExperienceShard shard) {
                    continue;
                }
                
                var distance = Vector2.Distance(abstractLevel.Player.Position, xp.Position);
                if (distance < 2f) {
                    collected.Add(shard);
                }
            }

            if (collected.Count == 0) {
                return;
            }

            var experience = 0;
            foreach (var shard in collected) {
                experience += shard.Value;
                abstractLevel.DestroyEntity(shard.Id);
            }

            abstractLevel.Post(new PlayerExperienceEvent {
                ExperienceDelta = experience,
            });
        }
    }
}