using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.EventBus {
    public abstract class AbstractEvent {
        public abstract DeliveryMethod NetworkDeliveryMethod { get; }
    }
}