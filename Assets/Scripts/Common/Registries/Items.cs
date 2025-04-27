using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common.Registries {
    public static class Items {
        public static readonly Item HealthBooster = Item.Create()
            .SetDescription("Increase your health by 12 per stack.")
            .AddComponent(ItemComponents.HealthIncrease, stackAmount => 12 * stackAmount)
            .Build();
        
        public static readonly Item KnightArmor = Item.Create()
            .SetDescription("Increase your damage resistance by 2 per stack.")
            .AddComponent(ItemComponents.GeneralDamageResistance, stackAmount => stackAmount * 2)
            .Build();
        
        public static readonly Item ThrowingKnives = Item.Create()
            .SetDescription("Throw knives - more stacks increase speed, count and damage.")
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.ThrowingKnife)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(3f * Mathf.Exp(-0.2f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min(stackAmount, 6))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.1f * stackAmount), 20))
            .Build();
        
        public static readonly Item ThrowingAxes = Item.Create()
            .SetDescription("Throw axes which penetrates enemies - more stacks increase speed, count and damage.")
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.ThrowingAxe)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(10f * Mathf.Exp(-0.2f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min(stackAmount, 3))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.2f * stackAmount), 20))
            .Build();
        
        public static readonly Item MagicStaff = Item.Create()
            .SetDescription("Magic staff, shoots magic missiles - more stacks increase speed, count and damage.")
            .AddComponent(ItemComponents.WeaponParticle, EntityTypes.MagicParticle)
            .AddComponent(ItemComponents.WeaponDelay, stackAmount => Mathf.Max(4f * Mathf.Exp(-0.4f * stackAmount), 0.1f))
            .AddComponent(ItemComponents.ParticleCount, stackAmount => Mathf.Min((stackAmount * 2) - 1, 16))
            .AddComponent(ItemComponents.DamageMultiplier, stackAmount => Mathf.Min(Mathf.Exp(0.05f * stackAmount), 20))
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