using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Common.Events {
    public class PlayerLevelUpEvent : AbstractEvent {
        public override byte Channel => 0;
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public int Level { get; set; }
        public int NextExperienceMilestone { get; set; }
    }
}