using Rover656.Survivors.Common.World;

namespace Rover656.Survivors.Framework
{
    public interface IHybridSystem<TGame> where TGame : AbstractHybridGame<TGame>
    {
        int SystemId { get; }
        
        bool IsActive { get; }
        
        // TODO: Update method which support deterministic behaviours using a current time, delta time and maybe
        //       compute delta using latency information to ensure when it arrives, it is current?

        void Update(TGame abstractLevel, float deltaTime);
    }
}