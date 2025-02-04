using System.Collections.Generic;
using Rover656.Survivors.Common.Items;

namespace Rover656.Survivors.Common {
    public class Item {
        private Dictionary<object, object> _components;

        private Item(Dictionary<object, object> components) {
            _components = components;
        }

        public bool HasComponent<T>(ItemComponentType<T> type) {
            return _components.ContainsKey(type);
        }

        public T GetComponent<T>(ItemComponentType<T> type) {
            return (T)_components[type];
        }

        public static Factory Create() {
            return new Factory();
        }

        public class Factory {
            private Dictionary<object, object> _components = new();

            public Factory AddComponent<T>(ItemComponentType<T> type, T component) {
                _components.Add(type, component);
                return this;
            }

            public Item Build() {
                return new Item(_components);
            }
        }
    }
}