using System;
using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.Entity {
    public class EntityType<T> : IEntityType where T : AbstractEntity {
        private readonly Func<T> _factory;

        public EntityType(Func<T> factory) {
            _factory = factory;
        }
        
        public AbstractEntity FromNetwork(NetDataReader reader) {
            var entity = _factory();
            entity.Deserialize(reader);
            return entity;
        }
    }
}