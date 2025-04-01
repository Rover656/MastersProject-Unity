using System.Collections.Generic;

namespace Rover656.Survivors.Framework.EventBus {
    public interface IEventBus {
        void Post<T>(T message) where T : AbstractEvent, new();
        
        public int MaxBulkPackets { get; }
        
        /**
         * Throws IllegalArgumentException if messages.Count < 1 or > MaxBulkPackets.
         */
        void PostMany<T>(IList<T> messages) where T : AbstractEvent, new();
    }
}