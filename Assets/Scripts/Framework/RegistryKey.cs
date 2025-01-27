namespace Rover656.Survivors.Framework {
    public class RegistryKey<T> {
        public string Name { get; set; }
        
        public RegistryKey(string name) {
            Name = name;
        }
    }
}