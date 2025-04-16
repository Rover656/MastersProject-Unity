using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events {
    public class PlayerCollectItemEvent : AbstractEvent, IPacketedEvent {
        public override byte Channel => 0;
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
        
        private class Packet {
            public int ItemId { get; set; }
            public int Count { get; set; }

            public ItemStack GetStack(IHybridGameAccess game) {
                var item = game.Registries.GetFrom(SurvivorsRegistries.Items, ItemId);
                return new ItemStack() {
                    Item = item,
                    Count = Count,
                };
            }
        }

        public void SendPacket(IPacketSender packetSender) {
            var packet = new Packet() {
                ItemId = packetSender.Registries.GetIdFrom(SurvivorsRegistries.Items, Stack.Item),
                Count = Stack.Count,
            };

            packetSender.Send(packet, NetworkDeliveryMethod, Channel);
        }
    }
}