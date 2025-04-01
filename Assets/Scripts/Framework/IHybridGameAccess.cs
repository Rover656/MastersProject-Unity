using LiteNetLib;
using Rover656.Survivors.Framework.EventBus;

namespace Rover656.Survivors.Framework {
    public interface IHybridGameAccess : IEventBus {
        
        IRegistryProvider Registries { get; }
        
        void Send<T>(T packet, DeliveryMethod deliveryMethod, byte channel = 0) where T : class, new();
    }
}