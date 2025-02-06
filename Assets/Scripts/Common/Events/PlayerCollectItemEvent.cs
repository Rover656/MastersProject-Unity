using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events {
    public class PlayerCollectItemEvent : AbstractEvent {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public ItemStack Stack { get; set; }

        public static void Register(NetPacketProcessor netPacketProcessor, Action<PlayerCollectItemEvent> handler) {
            // Use the underlying packet for transport.
            var reference = new PlayerCollectItemEvent();
            netPacketProcessor.SubscribeReusable<Packet, IHybridGameAccess>((p, game) => {
                reference.Stack = p.GetStack(game);
                handler(reference);
            });
        }
        
        private class Packet : INetSerializable {
            
            private int ItemId { get; set; }
            private int Count { get; set; }

            public ItemStack GetStack(IHybridGameAccess game) {
                var item = game.Registries.GetFrom(SurvivorsRegistries.Items, ItemId);
                return new ItemStack() {
                    Item = item,
                    Count = Count,
                };
            }
            
            public void Serialize(NetDataWriter writer) {
                
            }

            public void Deserialize(NetDataReader reader) {
                
            }
        }
    }
}