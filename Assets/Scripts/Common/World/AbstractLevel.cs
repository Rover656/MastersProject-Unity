using LiteNetLib;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Common.World
{
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel>
    {
        public int GameTime { get; set; }
        
        public Player Player { get; }
        
        protected AbstractLevel(NetManager netManager) : base(SurvivorsRegistries.Instance, netManager)
        {
            Player = AddNewEntity(new Player());

            // Register all systems.
            AddSystem(new PhysicsSystem());
        }
    }
}