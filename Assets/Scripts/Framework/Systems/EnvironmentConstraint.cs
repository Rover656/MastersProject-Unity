namespace Rover656.Survivors.Framework.Systems
{
    public enum EnvironmentConstraint
    {
        /// <summary>
        /// This system should only run locally.
        /// </summary>
        LocalOnly,
        
        /// <summary>
        /// This system has no preference as to which side it is run on.
        /// </summary>
        HybridAny,
        
        /// <summary>
        /// Prioritise keeping this system local.
        /// </summary>
        PreferLocal,
        
        /// <summary>
        /// Prioritise offloading this system.
        /// Useful for heavy systems that are not latency sensitive.
        /// </summary>
        PreferRemote,
    }
}