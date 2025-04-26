using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events
{
    public class EntityHealthChangedEvent : AbstractEvent, IPacketedEvent
    {
        public override byte Channel => 0;
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
    
        public Guid EntityId { get; set; }
        public int Delta { get; set; }
        public float? InvincibleUntil { get; set; }
        
        public static void Register(NetPacketProcessor netPacketProcessor, Action<EntityHealthChangedEvent> handler) {
            // Use the underlying packet for transport.
            EntityHealthChangedEvent reference;
            netPacketProcessor.SubscribeReusable<Packet, IHybridGameAccess>((p, _) => {
                reference = p.FormEvent();
                handler(reference);
            });
        }
        
        private class Packet {
            public Guid EntityId { get; set; }
            public int Delta { get; set; }
            public bool GainedInvincibilityFrames { get; set; }
            public float InvincibleUntil { get; set; }

            public Packet() {
            }
            
            public Packet(EntityHealthChangedEvent originalEvent) {
                EntityId = originalEvent.EntityId;
                Delta = originalEvent.Delta;
                GainedInvincibilityFrames = originalEvent.InvincibleUntil != null;
                InvincibleUntil = originalEvent.InvincibleUntil ?? 0;
            }

            public EntityHealthChangedEvent FormEvent() {
                return new EntityHealthChangedEvent() {
                    EntityId = EntityId,
                    Delta = Delta,
                    InvincibleUntil = GainedInvincibilityFrames ? InvincibleUntil : null,
                };
            }
        }

        public void SendPacket(IPacketSender game) {
            game.Send(new Packet(this), NetworkDeliveryMethod, Channel);
        }
    }
}