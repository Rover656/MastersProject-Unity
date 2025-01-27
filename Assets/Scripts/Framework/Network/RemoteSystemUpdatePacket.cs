namespace Rover656.Survivors.Framework.Network
{
    /// <summary>
    /// Packet sent to the remote server to enable or disable the remote system.
    /// The client can make its own decisions on whether a system is halted locally.
    /// </summary>
    public class RemoteSystemUpdatePacket
    {
        public int SystemId { get; set; }
        
        public bool IsEnabled { get; set; }
    }
}