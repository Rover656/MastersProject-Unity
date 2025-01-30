using System;
using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events
{
    public class EntityHealEvent : AbstractEvent
    {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public Guid EntityId { get; set; }
        public int Healing { get; set; }
    }
}