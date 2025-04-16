using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Common.Systems {
    public class ArbitraryLoadSystem : IGameSystem<AbstractLevel> {
        public GameSystemType Type => SystemTypes.ArbitraryLoad;
        
        public void Update(AbstractLevel abstractLevel, float deltaTime) {
            // Fake load to try and cause a performance impact.
            // counter exists to ensure this loop has a side effect
            ulong counter = 0;
            for (ulong i = 0; i < 50_000_000; i++)
            {
               counter += i;
            }
            
            // Debug.Log(counter);
        }
    }
}