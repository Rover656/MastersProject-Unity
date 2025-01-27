using LiteNetLib;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Server
{
    public class ServerLevel : AbstractLevel
    {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Remote;
        public override float NetworkDelay => 0f; // TODO
        
        public ServerLevel(NetManager netManager) : base(netManager)
        {
        }
    }
}