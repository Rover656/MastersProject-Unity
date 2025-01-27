using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.Network
{
    public class SpawnEntityPacket<TGame> : INetSerializable where TGame : AbstractHybridGame<TGame>
    {
        public AbstractEntity<TGame> Entity { get; set; }
        
        public void Serialize(NetDataWriter writer) {
            Entity.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader) {
            Entity.Deserialize(reader);
        }
    }
}