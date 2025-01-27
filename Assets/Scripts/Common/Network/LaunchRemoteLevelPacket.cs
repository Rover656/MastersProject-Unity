namespace Rover656.Survivors.Common.Network
{
    public class LaunchRemoteLevelPacket
    {
        public int RandomSeed { get; set; }
        public float GameTime { get; set; }
        
        // TODO: Send the stage index if we have multiple stages?
    }
}