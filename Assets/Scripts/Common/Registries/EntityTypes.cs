using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries {
    public static class EntityTypes {
        public static EntityType<ExperienceShard> BasicExperienceShard { get; } = new(() => new ExperienceShard(1));

        public static EntityType<Player> Player { get; } =
            new(() => new Player(), GeneralEntityTags.Player, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<Enemy> Bat { get; } = new(() => new Enemy(Bat, 6f, 5, 2, true, new()),
            GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<Enemy> RuneWizard { get; } = new(() => new Enemy(RuneWizard, 4f, 15, 3, false, new() {
                new ItemStack {
                    Item = Items.ThrowingKnives,
                    Count = 1,
                }
            }), GeneralEntityTags.Enemy,
            EnemyMovementTag.DistancedFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<WeaponParticle> ThrowingKnife { get; } = new(() =>
                new WeaponParticle(ThrowingKnife, ParticleMovementType.AimRandomTarget, 16f, 2, 2),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle, GeneralEntityTags.FaceMovementVector);

        public static EntityType<WeaponParticle> ThrowingAxe { get; } = new(() =>
                new WeaponParticle(ThrowingAxe, ParticleMovementType.RandomDirection, 12f, 4, 1),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle, GeneralEntityTags.FaceMovementVector);

        public static void Register(Registry<IEntityType> registry) {
            registry.Register("basic_experience_shard", BasicExperienceShard);
            registry.Register("player", Player);

            registry.Register("bat", Bat);
            registry.Register("rune_wizard", RuneWizard);

            registry.Register("throwing_knife", ThrowingKnife);
            registry.Register("throwing_axe", ThrowingAxe);
        }
    }
}