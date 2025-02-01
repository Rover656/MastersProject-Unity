using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.Entity {
    public interface IEntityType
    {
        // Tags are used to cluster entities in the level.
        public object[] Tags { get; }
        
        AbstractEntity FromNetwork(NetDataReader reader);
    }
}