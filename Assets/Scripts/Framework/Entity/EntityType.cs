using System;
using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.Entity {
    public class EntityType<T> : IEntityType where T : AbstractEntity {
        public object[] Tags { get; }
        private readonly Func<T> _factory;

        public EntityType(Func<T> factory, params object[] tags) {
            Tags = tags;
            _factory = factory;
        }

        public T Create()
        {
            return _factory();
        }

        public AbstractEntity FromNetwork(NetDataReader reader) {
            var entity = _factory();
            entity.Deserialize(reader);
            return entity;
        }
    }
}