using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Common.Systems
{
    /// <summary>
    /// The physics system handles entity movement & collision.
    /// This will not use Unity's systems for collision as the entities are not necessarily stored in a scene.
    /// </summary>
    public class PhysicsSystem : IHybridSystem<AbstractLevel>
    {
        public int SystemId => 1;
        
        public bool IsActive { get; set; }
        
        public void Update(AbstractLevel abstractLevel, float deltaTime)
        {
            // TODO: Add latency to delta time...
            
            // Apply movements
            foreach (var entity in abstractLevel.Entities)
            {
                // TODO: Collision checks.
                if (entity.Velocity.magnitude > 0)
                {
                    entity.SetPosition(entity.Position + (entity.Velocity * deltaTime));
                }
            }
        }
    }
}