using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Common.Registries
{
    public class SystemTypes
    {
        public static GameSystemType DumbFollower = new(EnvironmentConstraint.HybridAny, 0);
        public static GameSystemType Damage = new(EnvironmentConstraint.HybridAny, 0);
        public static GameSystemType Director = new(EnvironmentConstraint.PreferLocal, 0);
        public static GameSystemType ParticleLifetime = new(EnvironmentConstraint.PreferRemote, 0);
        public static GameSystemType Physics = new(EnvironmentConstraint.PreferLocal, 0);
        public static GameSystemType Weapon = new(EnvironmentConstraint.PreferLocal, 0);
        
        public static void Register(Registry<GameSystemType> registry) {
            registry.Register("dumb_follower", DumbFollower);
            registry.Register("damage", Damage);
            registry.Register("director", Director);
            registry.Register("particle_lifetime", ParticleLifetime);
            registry.Register("physics", Physics);
            registry.Register("weapon", Weapon);
        }
    }
}