using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Preview {
    /// <summary>
    /// Represents the state of a loot table after being processed by a pipeline,
    /// without actually performing any random rolls.
    /// </summary>
    public class LootTablePreview<T> {
        public List<LootPreviewEntry<T>> Entries { get; } = new();
        public float TotalWeight { get; internal set; }
    }

    /// <summary>
    /// A potential outcome in a loot table preview.
    /// </summary>
    public class LootPreviewEntry<T> {
        public T? Item { get; internal set; }
        public float OriginalWeight { get; internal set; }
        public float ModifiedWeight { get; internal set; }
        public IntRange OriginalQuantity { get; internal set; }
        public IntRange ModifiedQuantity { get; internal set; }
        public int OriginalIndex { get; internal set; }
        
        /// <summary>
        /// Probability of this entry being picked in a single weighted roll (0-1).
        /// </summary>
        public float Probability { get; internal set; }
    }
}
