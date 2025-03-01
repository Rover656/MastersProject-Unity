namespace Rover656.Survivors.Framework.Systems
{
    // NOTE: The enum order is important for priority sorting.
    public enum EnvironmentConstraint
    {
        /// <summary>
        /// This system should only run locally.
        /// </summary>
        LocalOnly = 0,
        
        /// <summary>
        /// This system has no preference as to which side it is run on.
        /// </summary>
        HybridAny = 1,
        
        /// <summary>
        /// Prioritise keeping this system local.
        /// </summary>
        PreferLocal = 2,
    }
}