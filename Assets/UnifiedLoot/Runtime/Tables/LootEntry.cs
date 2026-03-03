namespace NS.UnifiedLoot {
    /// <summary>
    /// Default implementation of <see cref="ILootEntry{T}"/> for code-defined loot tables.
    /// </summary>
    public readonly struct LootEntry<T> : ILootEntry<T> {
        public T? Item { get; }
        public float Weight { get; }
        public IntRange Quantity { get; }

        public LootEntry(T? item, float weight = 1f, IntRange? quantity = null) {
            Item = item;
            Weight = weight;
            Quantity = quantity ?? new IntRange(1);
        }

        public override string ToString() => $"{Item} (w:{Weight:F2}, q:{Quantity})";
    }

    public static class LootEntry {
        public static LootEntry<T> Create<T>(T item, float weight = 1f)
            => new(item, weight);

        public static LootEntry<T> Create<T>(T item, float weight, int quantity)
            => new(item, weight, new IntRange(quantity));

        public static LootEntry<T> Create<T>(T item, float weight, int minQuantity, int maxQuantity)
            => new(item, weight, new IntRange(minQuantity, maxQuantity));

        /// <summary>
        /// Creates an "empty" entry. When this entry is selected by a weighted roll,
        /// no item is added to the results — it represents an empty drop slot.
        /// </summary>
        public static LootEntry<T> Empty<T>(float weight = 1f) => new(default, weight);
    }
}