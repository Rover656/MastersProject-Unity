using System;
using UnityEngine;

namespace Rover656.Survivors.Framework.Network
{
    public class EntityMovementVectorUpdatePacket
    {
        public Guid EntityId { get; set; }
        public Vector2 MovementVector { get; set; }
    }
}