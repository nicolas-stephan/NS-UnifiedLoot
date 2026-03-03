using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// The working state passed through the loot pipeline.
    /// Strategies read from and write to this set during processing.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public class LootWorkingSet<T> {
        /// <summary>
        /// The source loot table being rolled against.
        /// </summary>
        public ILootTable<T>? SourceTable { get; internal set; }

        /// <summary>
        /// The current results. Strategies can add, remove, or modify entries.
        /// </summary>
        public List<LootResult<T>> Results { get; } = new();

        /// <summary>
        /// The random number generator to use for this roll.
        /// </summary>
        public IRandom Random { get; internal set; } = null!;

        /// <summary>
        /// Cached list of entries with their computed weights (for weighted selection).
        /// </summary>
        public List<WeightedEntry<T>> WeightedEntries { get; } = new();

        /// <summary>
        /// Total weight of all entries (sum of WeightedEntries weights).
        /// </summary>
        public float TotalWeight { get; internal set; }

        /// <summary>
        /// Whether to collect metadata (can be disabled for performance).
        /// </summary>
        public bool CollectMetadata { get; internal set; } = true;

        /// <summary>
        /// A free-form blackboard strategies can use to communicate intermediate data
        /// with each other within a single pipeline execution. Unlike <see cref="LootContext"/>,
        /// which is caller-owned and persists across calls, the blackboard is cleared between
        /// every execution.
        /// </summary>
        public Dictionary<string, object> Blackboard { get; } = new();

        /// <summary>
        /// Clears the working set for reuse.
        /// </summary>
        internal void Clear() {
            SourceTable = null;
            Results.Clear();
            WeightedEntries.Clear();
            TotalWeight = 0f;
            Blackboard.Clear();
        }

        /// <summary>
        /// Adds a result to the working set.
        /// </summary>
        public void AddResult(T item, int quantity, LootMetadata metadata = default) => Results.Add(new LootResult<T>(item, quantity, metadata));

        /// <summary>
        /// Adds a result from an entry, rolling quantity.
        /// Entries with a null item are silently skipped (empty/sentinel entries).
        /// </summary>
        public void AddResult(ILootEntry<T> entry, int entryIndex, float rollValue) {
            if (entry.Item is null) return; // empty sentinel entry — nothing dropped

            var quantity = entry.Quantity.Roll(Random);
            var metadata = CollectMetadata
                ? new LootMetadata {
                    SourceTableId = SourceTable?.Id ?? 0,
                    OriginalWeight = entry.Weight,
                    FinalWeight = entry.Weight,
                    RollValue = rollValue,
                    EntryIndex = entryIndex,
                }
                : default;
            Results.Add(new LootResult<T>(entry.Item!, quantity, metadata));
        }

        /// <summary>
        /// Picks one entry via weighted random selection and adds it as a result.
        /// Does nothing if there are no entries or total weight is zero.
        /// </summary>
        public void TryRollOneResult() {
            if (WeightedEntries.Count == 0 || TotalWeight <= 0f) return;

            var roll = Random.Range(0f, TotalWeight);
            foreach (var weighted in WeightedEntries) {
                if (roll > weighted.CumulativeWeight) continue;
                AddResult(weighted.Entry, weighted.Index, roll / TotalWeight);
                return;
            }
        }
    }

    /// <summary>
    /// An entry with its computed weight for weighted selection algorithms.
    /// </summary>
    public struct WeightedEntry<T> {
        public ILootEntry<T> Entry;
        public int Index;
        public float Weight;
        public float CumulativeWeight;
    }
}
