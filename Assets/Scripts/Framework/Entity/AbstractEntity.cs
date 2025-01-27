using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Network;
using UnityEngine;

namespace Rover656.Survivors.Framework.Entity {
    public abstract class AbstractEntity : INetSerializable {
        
        public abstract IEntityType Type { get; }
        
        public IHybridGameAccess Game { get; set; }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Vector2 Position { get; internal set; }
        public Vector2 MovementVector { get; internal set; }

        public abstract float MovementSpeed { get; }

        public Vector2 Velocity => MovementVector * MovementSpeed;

        public void SetPosition(Vector2 position) {
            // Skip unnecessary updates & thus network traffic.
            if (position.Equals(Position)) {
                return;
            }

            Position = position;

            // Fire movement event
            Game.OnEntityMoved(this);

            // Send update to the remote side.
            Game.Send(new EntityPositionUpdatePacket() {
                EntityId = Id,
                Position = position,
            }, DeliveryMethod.ReliableOrdered);
        }

        public void SetMovementVector(Vector2 movementVector) {
            if (movementVector.Equals(MovementVector)) {
                return;
            }

            MovementVector = movementVector;

            // Send update to remote
            Game.Send(new EntityMovementVectorUpdatePacket() {
                EntityId = Id,
                MovementVector = MovementVector,
            }, DeliveryMethod.ReliableOrdered);
        }

        protected virtual void SerializeAdditional(NetDataWriter writer) {
        }

        protected virtual void DeserializeAdditional(NetDataReader reader) {
        }

        public void Serialize(NetDataWriter writer) {
            writer.Put(Id);
            writer.Put(Position.x);
            writer.Put(Position.y);
            writer.Put(MovementVector.x);
            writer.Put(MovementVector.y);
            SerializeAdditional(writer);
        }

        public void Deserialize(NetDataReader reader) {
            Id = reader.GetGuid();
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            MovementVector = new Vector2(reader.GetFloat(), reader.GetFloat());
            DeserializeAdditional(reader);
        }
    }
}