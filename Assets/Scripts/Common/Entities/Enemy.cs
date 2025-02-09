using LiteNetLib.Utils;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities {
    public class Enemy : AbstractEntity, IDamageable, IDamageSource {
        public Enemy(IEntityType type, float movementSpeed, int maxHealth, int damage) {
            Type = type;
            MovementSpeed = movementSpeed;
            MaxHealth = maxHealth;
            Health = MaxHealth;
            Damage = damage;
        }

        public override IEntityType Type { get; }
        public override float MovementSpeed { get; }

        public override bool CanCollide => true;
        public override int PhysicsLayer => CollisionLayers.Enemies;

        public int Health { get; private set; }
        
        public float InvincibilityDuration => 0.25f;
        public float InvincibleUntil { get; private set; }

        public int MaxHealth { get; }

        public int DamagesPhysicsLayer => CollisionLayers.Player;
        public int Damage { get; }

        public void LocalSetHealth(int health) {
            Health = health;
        }

        public void LocalSetInvincibleUntil(float invincibleUntil) {
            InvincibleUntil = invincibleUntil;
        }

        protected override void SerializeAdditional(NetDataWriter writer) {
            base.SerializeAdditional(writer);
            writer.Put(Health);
            writer.Put(InvincibleUntil);
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            Health = reader.GetInt();
            InvincibleUntil = reader.GetInt();
        }
    }
}