using LiteNetLib.Utils;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Common.Entities
{
    public class Player : AbstractEntity, IDamageable {
        public override IEntityType Type => EntityTypes.Player;

        public override bool CanCollide => false;
        public override int PhysicsLayer => 0;
        
        public override float MovementSpeed => 32f;

        public int Health { get; private set; }
        
        public int MaxHealth => 12; // TODO

        public Player()
        {
            Health = MaxHealth;
        }

        public void LocalSetHealth(int health)
        {
            Health = health;
        }

        protected override void SerializeAdditional(NetDataWriter writer)
        {
            base.SerializeAdditional(writer);
            writer.Put(Health);
        }

        protected override void DeserializeAdditional(NetDataReader reader)
        {
            base.DeserializeAdditional(reader);
            Health = reader.GetInt();
        }
    }
}