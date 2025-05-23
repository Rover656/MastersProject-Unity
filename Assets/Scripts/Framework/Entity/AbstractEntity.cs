using System;
using LiteNetLib.Utils;
using Rover656.Survivors.Framework.Events;
using UnityEngine;

namespace Rover656.Survivors.Framework.Entity {
    public abstract class AbstractEntity : INetSerializable {
        
        public abstract IEntityType Type { get; }

        private IHybridGameAccess _game;

        public IHybridGameAccess Game {
            get => _game;
            set {
                _game = value;
                OnGameAttached();
            }
        }
        
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Vector2 Position { get; internal set; }
        public Vector2 Size => Vector2.one; // TODO: Configurable
        
        public Rect Bounds => new(Position, Size);
        
        public abstract bool CanCollide { get; }
        public abstract int PhysicsLayer { get; }
        public Vector2 MovementVector { get; internal set; }

        public abstract float MovementSpeed { get; }

        public Vector2 Velocity => MovementVector * MovementSpeed;

        private double? _seededRandomValue;

        public void SetPosition(Vector2 position) {
            // Skip unnecessary updates & thus network traffic.
            if (position.Equals(Position)) {
                return;
            }
            
            // Post update event.
            Game.Post(new EntityPositionChangedEvent {
                EntityId = Id,
                Position = position,
            });
        }

        public void SetMovementVector(Vector2 movementVector) {
            if (movementVector.Equals(MovementVector)) {
                return;
            }

            Game.Post(new EntityMovementVectorChangedEvent {
                EntityId = Id,
                MovementVector = movementVector,
            });
        }

        protected virtual void OnGameAttached() {
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

        public float GetOffset(float minInclusive, float maxInclusive) {
            // Only seed it once for performance.
            _seededRandomValue ??= new System.Random(Id.GetHashCode()).NextDouble();

            return (float)(_seededRandomValue * (maxInclusive - minInclusive) + minInclusive);
        }
    }
}