using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class Player : AbstractEntity {
        public override IEntityType Type => EntityTypes.Player;
        
        public override float MovementSpeed => 32f;
    }
}