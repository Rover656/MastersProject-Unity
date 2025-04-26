using LiteNetLib;
using LiteNetLib.Utils;

namespace Rover656.Survivors.Framework.EventBus {
    // Proxy class for sending IPacketedEvents in bulk.
    public class BulkEventPacketSender : IPacketSender {
        private readonly NetPacketProcessor _netPacketProcessor;
        private readonly NetDataWriter _writer;
        
        public IRegistryProvider Registries { get; }

        public BulkEventPacketSender(NetPacketProcessor netPacketProcessor, NetDataWriter writer, IRegistryProvider registryProvider) {
            _netPacketProcessor = netPacketProcessor;
            _writer = writer;
            Registries = registryProvider;
        }

        public void Send<T>(T packet, DeliveryMethod deliveryMethod, byte channel = 0) where T : class, new() {
            _netPacketProcessor.Write(_writer, packet);
        }
    }
}