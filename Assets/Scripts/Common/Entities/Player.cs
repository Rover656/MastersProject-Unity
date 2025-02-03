using LiteNetLib.Utils;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class Player : AbstractEntity, IDamageable {
        public override IEntityType Type => EntityTypes.Player;

        public override bool CanCollide => false;
        public override int PhysicsLayer => CollisionLayers.Player;
        
        public override float MovementSpeed => 32f;
        
        public int MaxHealth => 12; // TODO
        public int Health { get; private set; }
        
        public float InvincibilityDuration => 0.17f;
        public float InvincibleUntil { get; private set; }

        public Player()
        {
            Health = MaxHealth;
        }

        public void LocalSetHealth(int health)
        {
            Health = health;
        }

        public void LocalSetInvincibleUntil(float invincibleUntil) {
            InvincibleUntil = invincibleUntil;
        }

        protected override void SerializeAdditional(NetDataWriter writer)
        {
            base.SerializeAdditional(writer);
            writer.Put(Health);
            writer.Put(InvincibleUntil);
        }

        protected override void DeserializeAdditional(NetDataReader reader)
        {
            base.DeserializeAdditional(reader);
            Health = reader.GetInt();
            InvincibleUntil = reader.GetInt();
        }
    }
}