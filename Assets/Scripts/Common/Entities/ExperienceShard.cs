using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class ExperienceShard : AbstractEntity
    {
        public override IEntityType Type => EntityTypes.BasicExperienceShard;

        public override bool CanCollide => false;
        public override int PhysicsLayer => CollisionLayers.Player;
        public override float MovementSpeed => 0;

        public int Value { get; }

        public ExperienceShard(int value)
        {
            Value = value;
        }
    }
}