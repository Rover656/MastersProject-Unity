using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.Registries {
    public class Items {
        public static Item HealthBooster = Item.Create()
            .AddComponent(ItemComponents.HealthIncrease, stackAmount => 12 * stackAmount)
            .Build();
        
        public static Item KnightArmor = Item.Create()
            .AddComponent(ItemComponents.GeneralDamageResistance, stackAmount => stackAmount * 2)
            .Build();
        
        public static Item ThrowingKnives = Item.Create()
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.ThrowingKnife)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(3f * Mathf.Exp(-0.2f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min(stackAmount, 6))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.2f * stackAmount), 20))
            .Build();
        
        public static Item ThrowingAxes = Item.Create()
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.ThrowingAxe)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(5f * Mathf.Exp(-0.2f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min(stackAmount, 8))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.4f * stackAmount), 20))
            .Build();
        
        public static Item MagicStaff = Item.Create()
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.MagicParticle)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(4f * Mathf.Exp(-0.4f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min((stackAmount * 2) - 1, 16))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.2f * stackAmount), 20))
            .Build();
        
        public static void Register(Registry<Item> registry) {
            registry.Register("health_booster", HealthBooster);
            registry.Register("knight_armor", KnightArmor);
            registry.Register("throwing_knives", ThrowingKnives);
            registry.Register("throwing_axes", ThrowingAxes);
            registry.Register("magic_staff", MagicStaff);
        }
    }
}