using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events
{
    public class EntityHealthChangedEvent : AbstractEvent, INetSerializable
    {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
    
        public Guid EntityId { get; set; }
        public int Delta { get; set; }
        public float? InvincibleUntil { get; set; }
        
        public void Serialize(NetDataWriter writer) {
            writer.Put(EntityId);
            writer.Put(Delta);
            if (InvincibleUntil.HasValue) {
                writer.Put(true);
                writer.Put(InvincibleUntil.Value);
            } else {
                writer.Put(false);
            }
        }

        public void Deserialize(NetDataReader reader) {
            EntityId = reader.GetGuid();
            Delta = reader.GetInt();
            
            var hasInvincibleUntil = reader.GetBool();
            InvincibleUntil = hasInvincibleUntil ? reader.GetFloat() : null;
        }
    }
}