namespace Rover656.Survivors.Framework {
    public interface IRegistryProvider {
        bool Has<T>(RegistryKey<T> key);
        Registry<T> Get<T>(RegistryKey<T> key);

        // Shortcuts
        public int GetIdFrom<T>(RegistryKey<T> key, T entry) {
            return Get(key).GetId(entry);
        }
        
        public string GetNameFrom<T>(RegistryKey<T> key, T entry) {
            return Get(key).GetName(entry);
        }

        public T GetFrom<T>(RegistryKey<T> key, string name) {
            return Get(key).Get(name);
        }

        public T GetFrom<T>(RegistryKey<T> key, int id) {
            return Get(key).Get(id);
        }
    }
}