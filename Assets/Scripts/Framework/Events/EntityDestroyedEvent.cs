using System;
using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Framework.Events {
    public class EntityDestroyedEvent : AbstractEvent {
        public override byte Channel => 0;
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public Guid EntityId { get; set; }
    }
}