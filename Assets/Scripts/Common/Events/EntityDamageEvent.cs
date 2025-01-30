using System;
using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events
{
    public class EntityDamageEvent : AbstractEvent
    {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
    
        public Guid EntityId { get; set; }
        public int Damage { get; set; }
    }
}