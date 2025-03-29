using System.Collections.Generic;
using Rover656.Survivors.Common.Items;

namespace Rover656.Survivors.Common.Entities {
    public interface IEntityInventory {
        public IEnumerable<ItemStack> Inventory { get; }
    }
}