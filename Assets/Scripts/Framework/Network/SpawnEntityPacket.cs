namespace Rover656.Survivors.Framework.Network
{
    public class SpawnEntityPacket<TGame> where TGame : AbstractHybridGame<TGame>
    {
        public AbstractEntity<TGame> Entity { get; set; }
    }
}