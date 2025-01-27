using LiteNetLib;
using Rover656.Survivors.Framework.Entity;

namespace Rover656.Survivors.Framework {
    public interface IHybridGameAccess {
        
        IRegistryProvider Registries { get; }
        
        // Send packet
        void Send<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new();
        
        // Entity events
        void OnEntityAdded(AbstractEntity entity);
        void OnEntityMoved(AbstractEntity entity);
        void OnEntityRemoved(AbstractEntity entity);
    }
}