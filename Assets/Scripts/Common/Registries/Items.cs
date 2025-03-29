using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.Registries {
    public class Items {
        public static Item HealthBooster = Item.Create()
            .AddComponent(ItemComponents.HealthIncrease, (stackAmount) => 12 * stackAmount)
            .Build();
        
        public static Item ThrowingKnives = Item.Create()
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.ThrowingKnife)
            .AddComponent(ItemComponents.WeaponDelay, (stackAmount) => Mathf.Max(3f * Mathf.Exp(-0.2f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, (stackAmount) => Mathf.Min(stackAmount, 6))
            .AddComponent(ItemComponents.DamageMultiplier, (stackAmount) => Mathf.Min(3f * Mathf.Exp(0.2f * stackAmount), 20))
            .Build();
        
        public static void Register(Registry<Item> registry) {
            registry.Register("health_booster", HealthBooster);
            registry.Register("throwing_knives", ThrowingKnives);
        }
    }
}