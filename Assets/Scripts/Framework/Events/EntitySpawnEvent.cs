using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Framework.Events {
    public class EntitySpawnEvent : AbstractEvent, IPacketedEvent {
        
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public AbstractEntity Entity { get; set; }

        public static void Register(NetPacketProcessor netPacketProcessor, Action<EntitySpawnEvent> handler) {
            // Use the underlying packet for transport.
            var reference = new EntitySpawnEvent();
            netPacketProcessor.SubscribeReusable<Packet, IHybridGameAccess>((p, game) => {
                reference.Entity = p.CreateEntityFrom(game);
                handler(reference);
            });
        }

        private class Packet {
            public int EntityTypeId { get; set; }
            public byte[] EntityData { get; set; }

            public Packet() {
            }
            
            public Packet(AbstractEntity entity) {
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
        }

        public void SendPacket(IHybridGameAccess game) {
            game.Send(new Packet(Entity), NetworkDeliveryMethod);
        }
    }
}