using System.Collections.Generic;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities {
    public class Enemy : AbstractEntity, IDamageable, IDamageSource, IEntityInventory {
        public Enemy(IEntityType type, float movementSpeed, int maxHealth, int damage, bool isFlying, int killExperience,
            List<ItemStack> inventory) {
            Type = type;
            MovementSpeed = movementSpeed;
            _baseHealth = maxHealth;
            Health = MaxHealth;
            _baseDamage = damage;
            IsFlying = isFlying;
            KillExperience = killExperience;
            
            Inventory = inventory;
        }

        private int _baseHealth;
        private int _baseDamage;
        
        public IEnumerable<ItemStack> Inventory { get; }

        public override IEntityType Type { get; }
        public override float MovementSpeed { get; }

        public override bool CanCollide => true;
        
        public bool IsFlying { get; }
        public override int PhysicsLayer => CollisionLayers.Enemies;

        public int Health { get; private set; }
        
        public float InvincibilityDuration => 0.25f;
        public float InvincibleUntil { get; private set; }

        public int MaxHealth => _baseHealth * DifficultyMultiplier;

        public int DamagesPhysicsLayer => CollisionLayers.Player;
        public int Damage => _baseDamage * DifficultyMultiplier;
        
        public int KillExperience { get; }

        // Set before spawn or don't edit.
        public int DifficultyMultiplier { get; set; } = 1;

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
            writer.Put(DifficultyMultiplier);
        }

        protected override void DeserializeAdditional(NetDataReader reader) {
            base.DeserializeAdditional(reader);
            Health = reader.GetInt();
            InvincibleUntil = reader.GetInt();
            DifficultyMultiplier = reader.GetInt();
        }
    }
}