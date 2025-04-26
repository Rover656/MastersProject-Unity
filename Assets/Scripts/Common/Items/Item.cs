using System.Collections.Generic;

namespace Rover656.Survivors.Common.Items {
    public class Item {
        public string Description { get; }
        public bool IsInternalOnly { get; }
        private readonly Dictionary<object, object> _components;

        private Item(string description, bool isInternalOnly, Dictionary<object, object> components) {
            Description = description;
            IsInternalOnly = isInternalOnly;
            _components = components;
        }

        public bool HasComponent<T>(ItemComponentType<T> type) {
            return _components.ContainsKey(type);
        }

        public T GetComponent<T>(ItemComponentType<T> type) {
            return (T)_components[type];
        }

        public bool TryGetComponent<T>(ItemComponentType<T> type, out T value)
        {
            if (HasComponent(type))
            {
                value = GetComponent(type);
                return true;
            }

            value = default;
            return false;
        }

        public static Factory Create() {
            return new Factory();
        }

        public class Factory {
            private string _description = "No Description";
            private bool _isInternalOnly;
            private readonly Dictionary<object, object> _components = new();
            
            public Factory SetDescription(string description) {
                _description = description;
                return this;
            }
            
            public Factory InternalOnly() {
                _isInternalOnly = true;
                return this;
            }

            public Factory AddComponent<T>(ItemComponentType<T> type, T component) {
                _components.Add(type, component);
                return this;
            }

            public Item Build() {
                return new Item(_description, _isInternalOnly, _components);
            }
        }
    }
}