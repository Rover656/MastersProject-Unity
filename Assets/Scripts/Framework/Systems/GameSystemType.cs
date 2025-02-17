namespace Rover656.Survivors.Framework.Systems
{
    public class GameSystemType
    {
        public GameSystemType(EnvironmentConstraint environmentConstraint, int impactScore)
        {
            EnvironmentConstraint = environmentConstraint;
            ImpactScore = impactScore;
        }

        public EnvironmentConstraint EnvironmentConstraint { get; }
        
        public int ImpactScore { get; }
    }
}