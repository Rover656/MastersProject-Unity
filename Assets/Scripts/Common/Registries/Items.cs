using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Framework;

namespace Rover656.Survivors.Common.Registries {
    public class Items {
        public static Item HealthBooster = Item.Create()
            .AddComponent(ItemComponents.HealthIncrease, (stackAmount) => 12 * stackAmount)
            .Build();
        
        public static void Register(Registry<Item> registry) {
            registry.Register("health_booster", HealthBooster);
        }
    }
}