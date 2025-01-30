using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.Entity {
    public interface IEntityType
    {
        AbstractEntity FromNetwork(NetDataReader reader);
    }
}