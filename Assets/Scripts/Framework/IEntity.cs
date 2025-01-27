using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace Rover656.Survivors.Framework
{
    public interface IEntity : INetSerializable
    {
        /// <summary>
        /// Unique identifier for the entity, used across sides.
        /// </summary>
        Guid Id { get; set; }
        
        Vector2 Position { get; set; }
        Vector2 MovementVector { get; set; }
        
        float MovementSpeed { get; }
    }
}