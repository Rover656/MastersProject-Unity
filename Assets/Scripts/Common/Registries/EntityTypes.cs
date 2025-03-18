using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries
{
    public static class EntityTypes
    {
        public static EntityType<ExperienceShard> BasicExperienceShard { get; } = new(() => new ExperienceShard(1));

        public static EntityType<Player> Player { get; } =
            new(() => new Player(), GeneralEntityTags.Player, GeneralEntityTags.Damageable);

        public static EntityType<Enemy> Bat { get; } = new(() => new Enemy(Bat, 6f, 5, 2), GeneralEntityTags.Enemy,
            EnemyMovementTag.DumbFollower, GeneralEntityTags.Damager, GeneralEntityTags.Damageable);

        public static EntityType<WeaponParticle> ThrowingKnife { get; } = new(() =>
                new WeaponParticle(ThrowingKnife, ParticleMovementType.AimRandomTarget, 12f, 2, 1),
            GeneralEntityTags.Damager, GeneralEntityTags.Particle);

        public static void Register(Registry<IEntityType> registry)
        {
            registry.Register("basic_experience_shard", BasicExperienceShard);
            registry.Register("player", Player);
            registry.Register("bat", Bat);

            registry.Register("throwing_knife", ThrowingKnife);
        }
    }
}