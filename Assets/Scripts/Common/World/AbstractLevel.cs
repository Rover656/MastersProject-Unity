using LiteNetLib;
using Rover656.Survivors.Common.Systems;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Common.World
{
    public abstract class AbstractLevel : AbstractHybridGame<AbstractLevel>
    {
        public int GameTime { get; set; }
        
        public Player Player { get; }
        
        protected AbstractLevel(NetManager netManager) : base(netManager)
        {
            Player = AddNewEntity(new Player());

            AddSystem(new PhysicsSystem());
        }
    }
}