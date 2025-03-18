using LiteNetLib;

namespace Rover656.Survivors.Framework.EventBus {
    public abstract class AbstractEvent {
        public abstract DeliveryMethod NetworkDeliveryMethod { get; }
    }
}