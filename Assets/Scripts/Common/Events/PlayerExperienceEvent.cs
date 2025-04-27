using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events {
    public class PlayerExperienceEvent : AbstractEvent {
        public override byte Channel => 0;
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public int ExperienceDelta { get; set; }
    }
}