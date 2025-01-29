using System;
using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;
using UnityEngine;

namespace Rover656.Survivors.Framework.Events {
    public class EntityPositionChangedEvent : AbstractEvent {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableSequenced;
        
        public Guid EntityId { get; set; }
        
        public Vector2 Position { get; set; }
    }
}