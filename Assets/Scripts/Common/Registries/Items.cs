using Rover656.Survivors.Common.Items;

namespace Rover656.Survivors.Common.Registries {
    public class Items {
        public Item HealthBooster = Item.Create()
            .AddComponent(ItemComponents.HealthIncrease, (stackAmount) => 12 * stackAmount)
            .Build();
    }
}