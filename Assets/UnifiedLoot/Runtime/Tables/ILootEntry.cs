namespace NS.UnifiedLoot
{
    /// <summary>
    /// Represents a single entry in a loot table.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public interface ILootEntry<out T>
    {
        /// <summary>
        /// The item that can drop. May be <c>null</c> for "empty" entries
        /// (see <see cref="LootEntry.Empty{T}"/>). Strategies treat a null item as
        /// "nothing dropped from this slot."
        /// </summary>
        T? Item { get; }

        /// <summary>
        /// The weight/probability of this entry relative to others.
        /// Higher weights mean a higher chance of being selected.
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// The quantity range for this drop.
        /// </summary>
        IntRange Quantity { get; }
    }
}
