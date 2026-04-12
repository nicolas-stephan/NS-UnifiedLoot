namespace NS.UnifiedLoot {
    /// <summary>
    /// Represents a single item drop result from the loot system.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public readonly struct LootResult<T> {
        /// <summary>
        /// The dropped item.
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// The quantity of the item dropped.
        /// </summary>
        public readonly int Quantity;

        /// <summary>
        /// Metadata about the roll that produced this result.
        /// </summary>
        public readonly LootMetadata Metadata;

        public LootResult(T item, int quantity) {
            Item = item;
            Quantity = quantity;
            Metadata = default;
        }

        public LootResult(T item, int quantity, LootMetadata metadata) {
            Item = item;
            Quantity = quantity;
            Metadata = metadata;
        }

        public override string ToString() => Quantity == 1 ? $"{Item}" : $"{Item} x{Quantity}";
    }
}
