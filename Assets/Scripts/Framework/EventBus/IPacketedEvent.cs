using LiteNetLib;

namespace Rover656.Survivors.Framework.EventBus {
    public interface IPacketSender {
        IRegistryProvider Registries { get; }
        
        void Send<T>(T packet, DeliveryMethod deliveryMethod, byte channel = 0) where T : class, new();
    }
    
    public interface IPacketedEvent {
        void SendPacket(IPacketSender packetSender);
    }
}