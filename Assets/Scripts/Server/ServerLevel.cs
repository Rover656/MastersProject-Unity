using LiteNetLib;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;

namespace Rover656.Survivors.Server
{
    public class ServerLevel : AbstractLevel
    {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Remote;
        
        public float PeerNetworkDelay { get; set; }
        
        public override Environment Environment => Environment.Remote;

        public override float DeltaTime => base.DeltaTime + PeerNetworkDelay;

        public ServerLevel(NetManager netManager, NetPeer peer) : base(netManager)
        {
            NetPeer = peer;
        }
    }
}