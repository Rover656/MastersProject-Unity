using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Framework.Network
{
    public class SpawnEntityPacket : INetSerializable
    {
        public int EntityTypeId { get; set; }
        public byte[] EntityData { get; set; }

        public SpawnEntityPacket() {
        }

        public SpawnEntityPacket(AbstractEntity entity) {
            var entityTypeId = entity.Game.Registries.GetIdFrom(FrameworkRegistries.EntityTypes, entity.Type);
            
            EntityTypeId = entityTypeId;
            
            // Serialize data
            var writer = new NetDataWriter();
            entity.Serialize(writer);
            EntityData = writer.Data;
        }

        public AbstractEntity CreateEntityFrom(IHybridGameAccess game) {
            var entityType = game.Registries.GetFrom(FrameworkRegistries.EntityTypes, EntityTypeId);
            return entityType.FromNetwork(new NetDataReader(EntityData));
        }
        
        public void Serialize(NetDataWriter writer) {
            writer.Put(EntityTypeId);
            writer.Put(EntityData);
        }

        public void Deserialize(NetDataReader reader) {
            EntityTypeId = reader.GetInt();
            EntityData = reader.GetRemainingBytes();
        }
    }
}