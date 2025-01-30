using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries {
    public class EntityTypes {
        public static EntityType<Player> Player = new(() => new Player());

        public static EntityType<Enemy> Bat = new(() => new Enemy(Bat, 12f, 12));

        public static void Register(Registry<IEntityType> registry) {
            registry.Register("player", Player);
            registry.Register("bat", Bat);
        }
    }
}