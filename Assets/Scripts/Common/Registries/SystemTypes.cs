using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Common.Registries
{
    public static class SystemTypes
    {
        public static GameSystemType DumbFollower { get; } = new(EnvironmentConstraint.HybridAny, 1);
        public static GameSystemType DistancedFollower { get; } = new(EnvironmentConstraint.HybridAny, 1);
        public static GameSystemType Damage { get; } = new(EnvironmentConstraint.HybridAny, 3);
        public static GameSystemType Director { get; } = new(EnvironmentConstraint.PreferLocal, 0);
        public static GameSystemType Particle { get; } = new(EnvironmentConstraint.HybridAny, 0);
        public static GameSystemType Physics { get; } = new(EnvironmentConstraint.PreferLocal, 5);
        public static GameSystemType Weapon { get; } = new(EnvironmentConstraint.PreferLocal, 2);
        public static GameSystemType Experience { get; } = new(EnvironmentConstraint.HybridAny, 0);
        
        public static void Register(Registry<GameSystemType> registry) {
            registry.Register("dumb_follower", DumbFollower);
            registry.Register("distanced_follower", DistancedFollower);
            registry.Register("damage", Damage);
            registry.Register("director", Director);
            registry.Register("particle", Particle);
            registry.Register("physics", Physics);
            registry.Register("weapon", Weapon);
            registry.Register("experience", Experience);
        }
    }
}