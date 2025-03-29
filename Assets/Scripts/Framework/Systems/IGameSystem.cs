namespace Rover656.Survivors.Framework.Systems
{
    public interface IGameSystem<in TGame> where TGame : AbstractHybridGame<TGame>
    {
        GameSystemType Type { get; }
        
        // TODO: Update method which support deterministic behaviours using a current time, delta time and maybe
        //       compute delta using latency information to ensure when it arrives, it is current?

        void Update(TGame abstractLevel, float deltaTime);
    }
}