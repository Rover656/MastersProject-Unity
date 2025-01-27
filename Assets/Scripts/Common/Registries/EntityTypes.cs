using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Registries {
    public class EntityTypes {
        public static EntityType<Player> Player = new(() => new Player());

        public static void Register(Registry<IEntityType> registry) {
            registry.Register("player", Player);
        }
    }
}