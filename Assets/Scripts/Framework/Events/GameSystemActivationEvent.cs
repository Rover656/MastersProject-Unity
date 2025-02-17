using System;
using LiteNetLib;
using LiteNetLib.Utils;
using NUnit;
using Rover656.Survivors.Framework.EventBus;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Framework.Events
{
    public class GameSystemActivationEvent : AbstractEvent, IPacketedEvent
    {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public GameSystemType Type { get; set; }
        public Environment ActiveEnvironment { get; set; }
        
        public static void Register(NetPacketProcessor netPacketProcessor, Action<GameSystemActivationEvent> handler) {
            // Use the underlying packet for transport.
            var reference = new GameSystemActivationEvent();
            netPacketProcessor.SubscribeReusable<Packet, IHybridGameAccess>((p, game) => {
                reference.Type = p.GetType(game);
                reference.ActiveEnvironment = p.ActiveEnvironment;
                handler(reference);
            });
        }

        private class Packet {
            public int GameSystemTypeId { get; set; }
            public Environment ActiveEnvironment { get; set; }

            public Packet() {
            }
            
            public Packet(IHybridGameAccess game, GameSystemType type, Environment activeEnvironment) {
                GameSystemTypeId = game.Registries.GetIdFrom(FrameworkRegistries.GameSystemTypes, type);
                ActiveEnvironment = activeEnvironment;
            }

            public GameSystemType GetType(IHybridGameAccess game) {
                return game.Registries.GetFrom(FrameworkRegistries.GameSystemTypes, GameSystemTypeId);
            }
        }

        public void SendPacket(IHybridGameAccess game) {
            game.Send(new Packet(game, Type, ActiveEnvironment), NetworkDeliveryMethod);
        }
    }
}