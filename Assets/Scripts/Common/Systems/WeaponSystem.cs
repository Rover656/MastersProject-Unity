using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class WeaponSystem : IGameSystem<AbstractLevel> {
        public GameSystemType Type => SystemTypes.Weapon;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            var player = abstractLevel.Player;
            if (player is null)
            {
                return;
            }

            foreach (var itemStack in player.Inventory)
            {
                if (itemStack.Item.TryGetComponent(ItemComponents.WeaponParticle, out var particleType) &&
                    itemStack.Item.TryGetComponent(ItemComponents.WeaponDelay, out var weaponSpeedGetter))
                {
                    if (abstractLevel.EveryNSeconds(weaponSpeedGetter(itemStack.Count)))
                    {
                        int count = 1;
                        if (itemStack.Item.TryGetComponent(ItemComponents.ParticleCount, out var particleCountGetter))
                        {
                            count = particleCountGetter(itemStack.Count);
                        }
                        
                        Debug.Log($"Spawning {count} particles at {abstractLevel.GameTime}");
                        
                        // TODO: Spread the particles somehow?
                        for (int i = 0; i < count; i++)
                        {
                            var particle = particleType.Create();
                            particle.VolleyNumber = i;
                            abstractLevel.AddNewEntity(particle, player.Position);
                        }
                    }
                }
            }
        }
    }
}