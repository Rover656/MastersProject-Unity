using System;
using UnityEngine;

namespace Rover656.Survivors.Framework.Network
{
    public class EntityPositionUpdatePacket
    {
        public Guid EntityId { get; set; }
        public Vector2 Position { get; set; }
    }
}