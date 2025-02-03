namespace Rover656.Survivors.Common.Entities
{
    public interface IDamageable
    {
        public int Health { get; }
        public int MaxHealth { get; }
        
        public float InvincibilityDuration { get; }
        
        public float InvincibleUntil { get; }

        // Used by network handler.
        void LocalSetHealth(int health);
        void LocalSetInvincibleUntil(float invincibleUntil);
    }
}