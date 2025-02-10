namespace Rover656.Survivors.Framework.EventBus {
    public interface IEventBus {
        void Post<T>(T message) where T : AbstractEvent, new();
    }
}