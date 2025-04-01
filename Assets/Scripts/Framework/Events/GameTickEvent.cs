using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Framework.Events
{
    public class GameTickEvent : AbstractEvent
    {
        public override byte Channel => 0;
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.Sequenced;
        
        public byte[] MetaData { get; set; }
    }
}