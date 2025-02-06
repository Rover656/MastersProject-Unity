using LiteNetLib.Utils;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities {
    // TODO: Support for a particle that dies after contact?
    public class PlayerWeaponParticle : AbstractEntity, IDamageSource {
        public override IEntityType Type { get; }
        public override bool CanCollide => false;
        public override int PhysicsLayer => CollisionLayers.Player;
        public override float MovementSpeed { get; }
        public int DamagesPhysicsLayer => CollisionLayers.Enemies;
        public int Damage { get; private set; }
        
        private int Lifetime { get; set; }
        public float AliveUntil { get; private set; } = -1;

        public PlayerWeaponParticle(IEntityType type, float movementSpeed, int damage, int lifetime) {
            Type = type;
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
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            AliveUntil = reader.GetFloat();
        }
    }
}