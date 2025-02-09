using System;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Items;
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
        
        public int MaxHealth => 12 + _healthIncrease;
        public int Health { get; private set; }
        
        public float InvincibilityDuration => 0.25f;
        public float InvincibleUntil { get; private set; }

        private readonly List<ItemStack> _inventory = new();
        
        // Cached properties
        private int _healthIncrease = 0;
        private int _totalDamageResistance = 0;

        public Player()
        {
            Health = MaxHealth;
        }

        public int CalculateDamageTaken(int originalDamage) {
            return Math.Max(0, originalDamage - _totalDamageResistance);
        }

        public void LocalAddItem(ItemStack stack) {
            var added = false;
            foreach (var existingStack in _inventory) {
                if (existingStack.Item != stack.Item) continue;
                existingStack.Count += stack.Count;
                added = true;
                break;
            }

            if (!added) {
                _inventory.Add(stack);
            }
        }

        public void LocalSetHealth(int health)
        {
            Health = health;
        }

        public void LocalSetInvincibleUntil(float invincibleUntil) {
            InvincibleUntil = invincibleUntil;
        }
        
        // Cache item improvements.
        private void ProcessItems() {
            _healthIncrease = 0;
            _totalDamageResistance = 0;
            
            foreach (var stack in _inventory) {
                if (stack.Item.HasComponent(ItemComponents.HealthIncrease)) {
                    var getter = stack.Item.GetComponent(ItemComponents.HealthIncrease);
                    _healthIncrease += getter(stack.Count);
                }
                
                if (stack.Item.HasComponent(ItemComponents.GeneralDamageResistance)) {
                    var resistanceGetter = stack.Item.GetComponent(ItemComponents.GeneralDamageResistance);
                    _totalDamageResistance += resistanceGetter(stack.Count);
                }
            }
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