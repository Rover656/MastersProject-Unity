using System;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class Player : AbstractEntity, IDamageable, IEntityInventory {
        public override IEntityType Type => EntityTypes.Player;

        public override bool CanCollide => false;
        public override int PhysicsLayer => CollisionLayers.Player;
        
        public override float MovementSpeed => 16f;
        
        public int MaxHealth => 32 + _healthIncrease;
        public int Health { get; private set; }
        
        public float InvincibilityDuration => 0.25f;
        public float InvincibleUntil { get; private set; }
        
        public int Experience { get; set; }
        public int Level { get; set; } = 1;

        public int NextExperienceMilestone { get; set; } = 4;

        private readonly List<ItemStack> _inventory = new();

        public IEnumerable<ItemStack> Inventory => _inventory;
        
        // Cached properties
        private int _healthIncrease = 0;
        private int _totalDamageResistance = 0;

        public Player()
        {
            Health = MaxHealth;
            _inventory.Add(new ItemStack()
            {
                Item = Registries.Items.ThrowingKnives,
                Count = 1,
            });
        }

        public int CalculateDamageTaken(int originalDamage) {
            // No immunity, only reduce to 1 damage.
            return Math.Max(1, originalDamage - _totalDamageResistance);
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
        
        // Cache buffs
        public void UpdateStats() {
            _healthIncrease = 6 * (Level - 1);
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
            
            writer.Put(_inventory.Count);
            foreach (var item in _inventory) {
                writer.Put(SurvivorsRegistries.Instance.Get(SurvivorsRegistries.Items).GetId(item.Item));
                writer.Put(item.Count);
            }
        }

        protected override void DeserializeAdditional(NetDataReader reader)
        {
            base.DeserializeAdditional(reader);
            Health = reader.GetInt();
            InvincibleUntil = reader.GetInt();

            int itemCount = reader.GetInt();
            _inventory.Clear();
            for (int i = 0; i < itemCount; i++) {
                int id = reader.GetInt();
                int count = reader.GetInt();
                
                var item = SurvivorsRegistries.Instance.Get(SurvivorsRegistries.Items).Get(id);
                _inventory.Add(new ItemStack {
                    Item = item,
                    Count = count,
                });
            }
        }
    }
}