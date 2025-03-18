using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Items {
    public static class ItemComponents {
        public static ItemComponentType<StackScaled<int>> HealthIncrease { get; } = new();
        public static ItemComponentType<StackScaled<float>> GeneralDamageIncrease { get; } = new();
        public static ItemComponentType<StackScaled<int>> GeneralDamageResistance { get; } = new();

        public static ItemComponentType<EntityType<WeaponParticle>> WeaponParticle { get; } = new();
        public static ItemComponentType<StackScaled<float>> WeaponDelay { get; } = new();
        public static ItemComponentType<StackScaled<int>> ParticleCount { get; } = new();
    }
}