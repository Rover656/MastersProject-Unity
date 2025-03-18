using System;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities {
    // TODO: Support for a particle that dies after contact?
    public class WeaponParticle : AbstractEntity, IDamageSource {
        public override IEntityType Type { get; }
        public override bool CanCollide => false;
        public override int PhysicsLayer => CollisionLayers.Player;
        public int DamagesPhysicsLayer => CollisionLayers.Enemies;
        public int Damage { get; private set; }
        
        public ParticleMovementType MovementType { get; }
        public override float MovementSpeed { get; }
        
        private int Lifetime { get; set; }
        public float AliveUntil { get; private set; } = -1;
        
        // Assigned at generation time, can be used to make the particle act differently on nth spawn.
        public int VolleyNumber { get; set; }
        
        public Guid? TargetEntityId { get; set; }

        public WeaponParticle(IEntityType type, ParticleMovementType movementType, float movementSpeed, int damage, int lifetime) {
            Type = type;
            MovementType = movementType;
            MovementSpeed = movementSpeed;
            Damage = damage;
            Lifetime = lifetime;
        }

        protected override void OnGameAttached() {
            base.OnGameAttached();
            
            // Gross cast :/
            if (AliveUntil < 0) {
                AliveUntil = ((AbstractLevel)Game).GameTime + Lifetime;
            }
        }

        protected override void SerializeAdditional(NetDataWriter writer) {
            base.SerializeAdditional(writer);
            writer.Put(AliveUntil);
            writer.Put(VolleyNumber);

            writer.Put(TargetEntityId.HasValue);
            if (TargetEntityId.HasValue)
            {
                writer.Put(TargetEntityId.Value);
            }
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            AliveUntil = reader.GetFloat();
            VolleyNumber = reader.GetInt();

            bool hasTarget = reader.GetBool();
            if (hasTarget)
            {
                TargetEntityId = reader.GetGuid();
            }
        }
    }
}