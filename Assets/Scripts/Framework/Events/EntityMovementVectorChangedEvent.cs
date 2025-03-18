using System;
using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;
using UnityEngine;

namespace Rover656.Survivors.Framework.Events {
    public class EntityMovementVectorChangedEvent : AbstractEvent {
        public override DeliveryMethod NetworkDeliveryMethod => DeliveryMethod.ReliableOrdered;
        
        public Guid EntityId { get; set; }
        public Vector2 MovementVector { get; set; }
    }
}