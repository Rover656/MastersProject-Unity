using LiteNetLib;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Client
{
    public class ClientLevel : AbstractLevel
    {

        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Local;
        
        // As the local system, we do not need to account for networking delay.
        public override float NetworkDelay => 0;

        private readonly ClientLevelManager _clientLevelManager;
        
        public ClientLevel(NetManager netManager, ClientLevelManager clientLevelManager) : base(netManager)
        {
            _clientLevelManager = clientLevelManager;
        }

        protected override void OnEntityAdded(AbstractEntity<AbstractLevel> entity)
        {
            // Spawn in Unity world.
            _clientLevelManager.SpawnEntity(entity);
        }
    }
}