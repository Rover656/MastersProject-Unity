using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Framework.Events
{
    public class GameTickEvent : AbstractEvent
    {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableSequenced;
        
        public byte[] MetaData { get; set; }
    }
}