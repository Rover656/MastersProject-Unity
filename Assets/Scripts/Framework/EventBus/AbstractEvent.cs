using LiteNetLib;

namespace Rover656.Survivors.Framework.EventBus {
    public abstract class AbstractEvent {
        // Channel and Delivery Method should be the same across all packets of the same type.
        public abstract byte Channel { get; }
        public abstract DeliveryMethod NetworkDeliveryMethod { get; }
    }
}