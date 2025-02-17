using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Framework {
    public static class FrameworkRegistries {
        public static RegistryKey<IEntityType> EntityTypes = new("entity_type");
        public static RegistryKey<GameSystemType> GameSystemTypes = new("game_system");
    }
}