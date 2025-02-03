using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries {
    public class EntityTypes {
        public static EntityType<Player> Player { get; } = new(() => new Player(), GeneralEntityTags.Player, GeneralEntityTags.Damageable);

        public static EntityType<Enemy> Bat { get; } = new(() => new Enemy(Bat, 6f, 12, 2), EnemyMovementTag.DumbFollower, GeneralEntityTags.Damageable);

        public static void Register(Registry<IEntityType> registry) {
            registry.Register("player", Player);
            registry.Register("bat", Bat);
        }
    }
}