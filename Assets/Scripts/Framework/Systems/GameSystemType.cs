using System;

namespace Rover656.Survivors.Framework.Systems
{
    public class GameSystemType : IComparable
    {
        public GameSystemType(EnvironmentConstraint environmentConstraint, int impactScore)
        {
            EnvironmentConstraint = environmentConstraint;
            ImpactScore = impactScore;
        }

        public EnvironmentConstraint EnvironmentConstraint { get; }

        public int ImpactScore { get; }

        public int CompareTo(object obj)
        {
            // Compare based on environment constraint first, then by impact score.
            // PreferLocal > HybridAny > LocalOnly
            if (obj is not GameSystemType other) return 0;

            return EnvironmentConstraint == other.EnvironmentConstraint
                ? ImpactScore.CompareTo(other.ImpactScore)
                : EnvironmentConstraint.CompareTo(other.EnvironmentConstraint);
        }
    }
}