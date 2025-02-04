namespace Rover656.Survivors.Common.Items {
    public static class ItemComponents {
        public static ItemComponentType<StackScaled<int>> HealthIncrease { get; } = new();
        public static ItemComponentType<StackScaled<float>> GeneralDamageIncrease { get; } = new();
        public static ItemComponentType<StackScaled<int>> GeneralDamageResistance { get; } = new();
    }
}