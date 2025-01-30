using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class Enemy : AbstractEntity, IDamageable {
        public Enemy(IEntityType type, float movementSpeed, int maxHealth)
        {
            Type = type;
            MovementSpeed = movementSpeed;
            MaxHealth = maxHealth;
            Health = MaxHealth;
        }

        public override IEntityType Type { get; }
        public override float MovementSpeed { get; }

        public int Health { get; private set; }

        public int MaxHealth { get; }
        
        public void LocalSetHealth(int health)
        {
            Health = health;
        }
    }
}