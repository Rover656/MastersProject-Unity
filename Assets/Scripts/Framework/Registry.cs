using System;
using System.Collections.Generic;

namespace Rover656.Survivors.Framework {
    public class Registry<T> {
        private int _nextId;

        private readonly List<T> _entries = new();
        private readonly Dictionary<string, T> _byName = new();
        private readonly Dictionary<int, T> _byId = new();
        private readonly Dictionary<T, int> _idLookup = new();
        private readonly Dictionary<T, string> _nameLookup = new();
        
        public RegistryKey<T> Key { get; }

        public Registry(RegistryKey<T> key) {
            Key = key;
        }

        public T Get(int id) {
            return _byId[id];
        }

        public T Get(string name) {
            return _byName[name];
        }

        public int GetId(T entry) {
            return _idLookup[entry];
        }

        public string GetName(T entry) {
            return _nameLookup[entry];
        }

        public TEntry Register<TEntry>(String name, TEntry entry) where TEntry : T {
            var id = ++_nextId;
            _entries.Add(entry);
            _byName.Add(name, entry);
            _byId.Add(id, entry);
            _idLookup.Add(entry, id);
            _nameLookup.Add(entry, name);
            return entry;
        }
    }
}