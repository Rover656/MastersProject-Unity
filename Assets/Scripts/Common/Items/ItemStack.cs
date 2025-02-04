namespace Rover656.Survivors.Common.Items {
    public record ItemStack {
        public Item Item { get; set; }
        
        public int Count { get; set; }
    }
}