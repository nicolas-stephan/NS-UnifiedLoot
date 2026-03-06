namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Core {
    /// <summary>
    /// Metadata about a loot roll, useful for debugging and analytics.
    /// </summary>
    public readonly struct LootMetadata {
        /// <summary>
        /// The ID of the source table (faster than string comparison).
        /// </summary>
        public int SourceTableId { get; init; }

        /// <summary>
        /// The original weight of the entry before any modifications.
        /// </summary>
        public float OriginalWeight { get; init; }

        /// <summary>
        /// The final weight after all modifiers were applied.
        /// </summary>
        public float FinalWeight { get; init; }

        /// <summary>
        /// The actual roll value that resulted in this drop (0-1).
        /// </summary>
        public float RollValue { get; init; }

        /// <summary>
        /// Index of the entry in the source table.
        /// </summary>
        public int EntryIndex { get; init; }

        public override string ToString() =>
            $"[Table#{SourceTableId} Entry#{EntryIndex}] Weight: {OriginalWeight:F3} -> {FinalWeight:F3}, Roll: {RollValue:F3}";
    }
}