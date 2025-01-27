using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Framework {
    public static class FrameworkRegistries {
        public static RegistryKey<IEntityType> EntityTypes = new RegistryKey<IEntityType>("entity_type");
    }
}