using System.Collections.Generic;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Common.Registries {
    public class SurvivorsRegistries : IRegistryProvider {
        
        public static RegistryKey<Item> Items { get; } = new("items");
        
        public static SurvivorsRegistries Instance { get; } = new();

        private readonly Dictionary<string, object> _registries = new();
        
        private SurvivorsRegistries() {
            var entityTypes = CreateRegistry(FrameworkRegistries.EntityTypes);
            EntityTypes.Register(entityTypes);
            
            var systemTypes = CreateRegistry(FrameworkRegistries.GameSystemTypes);
            SystemTypes.Register(systemTypes);

            var items = CreateRegistry(Items);
            Registries.Items.Register(items);
        }

        private Registry<T> CreateRegistry<T>(RegistryKey<T> registryKey) {
            var registry = new Registry<T>(registryKey);
            _registries.Add(registry.Key.Name, registry);
            return registry;
        }

        public bool Has<T>(RegistryKey<T> key) {
            return _registries.ContainsKey(key.Name);
        }

        public Registry<T> Get<T>(RegistryKey<T> key) {
            return (Registry<T>)_registries[key.Name];
        }
    }
}