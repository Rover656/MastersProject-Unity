namespace Rover656.Survivors.Framework.EventBus {
    public interface IPacketedEvent {
        void SendPacket(IHybridGameAccess game);
    }
}