using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class WeaponSystem : IGameSystem<AbstractLevel> {
        public GameSystemType Type => SystemTypes.Weapon;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            if (!abstractLevel.EntitiesByTag.TryGetValue(GeneralEntityTags.Inventory, out var entitiesWithInventories))
            {
                return;
            }

            foreach (var entity in entitiesWithInventories) {
                if (entity is IEntityInventory inventoryEntity) {
                    foreach (var itemStack in inventoryEntity.Inventory)
                    {
                        if (itemStack.Item.TryGetComponent(ItemComponents.WeaponParticle, out var particleType) &&
                            itemStack.Item.TryGetComponent(ItemComponents.WeaponDelay, out var weaponSpeedGetter)) {
                            
                            float weaponSpeed = weaponSpeedGetter(itemStack.Count);
                            float offset = entity.GetOffset(weaponSpeed * -0.5f, weaponSpeed * 0.5f);
                            
                            if (abstractLevel.EveryNSeconds(weaponSpeed, offset))
                            {
                                int count = 1;
                                if (itemStack.Item.TryGetComponent(ItemComponents.ParticleCount, out var particleCountGetter))
                                {
                                    count = particleCountGetter(itemStack.Count);
                                }
                                
                                float damageMultiplier = 1;
                                if (itemStack.Item.TryGetComponent(ItemComponents.DamageMultiplier,
                                        out var damageMultiplierGetter))
                                {
                                    damageMultiplier = damageMultiplierGetter(itemStack.Count);
                                }
                        
                                // TODO: Spread the particles somehow?
                                for (int i = 0; i < count; i++)
                                {
                                    var particle = particleType.Create();
                                    particle.IsPlayerParticle = abstractLevel.HasTag(entity, GeneralEntityTags.Player);
                                    particle.VolleyNumber = i;
                                    particle.Damage = Mathf.RoundToInt(particle.Damage * damageMultiplier);
                                    abstractLevel.AddNewEntity(particle, entity.Position);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}