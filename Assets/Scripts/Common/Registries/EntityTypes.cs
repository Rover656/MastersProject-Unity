using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries {
    public static class EntityTypes {
        public static EntityType<ExperienceShard> BasicExperienceShard { get; } = new(() => new ExperienceShard(5), GeneralEntityTags.Experience);
        
        public static EntityType<Player> Player { get; } =
            new(() => new Player(), GeneralEntityTags.Player, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<Enemy> Bat { get; } = new(() => new Enemy(Bat, 4f, 2, 2, true, 1, new()),
            GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);
        
        public static EntityType<Enemy> Ghost { get; } = new(() => new Enemy(Ghost, 5f, 2, 4, true, 3, new()),
            GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);
        
        public static EntityType<Enemy> VileGhost { get; } = new(() => new Enemy(VileGhost, 6f, 8, 6, true, 5, new()),
            GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);
        
        public static EntityType<Enemy> Spider { get; } = new(() => new Enemy(Spider, 8f, 8, 5, false, 5, new()),
            GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<Enemy> RuneWizard { get; } = new(() => new Enemy(RuneWizard, 1f, 8, 3, false, 7, new() {
                new ItemStack {
                    Item = Items.MagicStaff,
                    Count = 1,
                }
            }), GeneralEntityTags.Enemy,
            EnemyMovementTag.DistancedFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<Enemy> ElderWizard { get; } = new(() => new Enemy(ElderWizard, 0.5f, 12, 5, false, 10, new() {
                new ItemStack {
                    Item = Items.MagicStaff,
                    Count = 2,
                }
            }), GeneralEntityTags.Enemy,
            EnemyMovementTag.DistancedFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable, GeneralEntityTags.Inventory);

        public static EntityType<WeaponParticle> ThrowingKnife { get; } = new(() =>
                new WeaponParticle(ThrowingKnife, ParticleMovementType.AimRandomTarget, 16f, 2, 2),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle, GeneralEntityTags.FaceMovementVector, GeneralEntityTags.DamagerDestroyOnContact);

        public static EntityType<WeaponParticle> ThrowingAxe { get; } = new(() =>
                new WeaponParticle(ThrowingAxe, ParticleMovementType.RandomDirection, 12f, 4, 1),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle, GeneralEntityTags.FaceMovementVector);

        public static EntityType<WeaponParticle> MagicParticle { get; } = new(() =>
                new WeaponParticle(MagicParticle, ParticleMovementType.AimRandomTarget, 16f, 6, 1),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle, GeneralEntityTags.FaceMovementVector, GeneralEntityTags.DamagerDestroyOnContact);

        public static void Register(Registry<IEntityType> registry) {
            registry.Register("basic_experience_shard", BasicExperienceShard);
            registry.Register("player", Player);

            registry.Register("bat", Bat);
            registry.Register("ghost", Ghost);
            registry.Register("vile_ghost", VileGhost);
            registry.Register("spider", Spider);
            registry.Register("rune_wizard", RuneWizard);
            registry.Register("elder_wizard", ElderWizard);

            registry.Register("throwing_knife", ThrowingKnife);
            registry.Register("throwing_axe", ThrowingAxe);
            registry.Register("magic_particle", MagicParticle);
        }
    }
}