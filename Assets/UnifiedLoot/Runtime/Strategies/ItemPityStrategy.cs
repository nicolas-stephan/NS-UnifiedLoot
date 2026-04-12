using System;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// A strategy that tracks pity for specific items rather than the whole table.
    /// If a tracked item hasn't dropped for a certain number of rolls, it is guaranteed.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public class ItemPityStrategy<T> : ILootGeneratorStrategy<T>, IResettable {
        private readonly IPityTracker _tracker;
        private readonly Func<T, int> _idExtractor;
        private readonly Dictionary<int, PityConfig> _configs = new();

        private class PityConfig {
            public T Item;
            public int Threshold;
        }

        /// <summary>
        /// Creates an item pity strategy.
        /// </summary>
        /// <param name="idExtractor">A function to extract a unique integer ID from an item.</param>
        /// <param name="tracker">The tracker to use. When <c>null</c>, creates its own tracker instance.</param>
        public ItemPityStrategy(Func<T, int> idExtractor, IPityTracker? tracker = null) {
            _idExtractor = idExtractor;
            _tracker = tracker ?? new PityTracker();
        }

        /// <summary>
        /// Configures pity for a specific item.
        /// </summary>
        /// <param name="item">The item to track.</param>
        /// <param name="threshold">Number of failures before the item is guaranteed.</param>
        public void AddTrackedItem(T item, int threshold) {
            var id = _idExtractor(item);
            _configs[id] = new PityConfig { Item = item, Threshold = threshold };
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            // 1. Identify what dropped in this roll
            var droppedIds = new HashSet<int>();
            foreach (var result in workingSet.Results) {
                if (result.Item != null) {
                    droppedIds.Add(_idExtractor(result.Item));
                }
            }

            // 2. Update pity for all tracked items
            foreach (var kvp in _configs) {
                var itemId = kvp.Key;
                var config = kvp.Value;

                if (droppedIds.Contains(itemId)) {
                    _tracker.Record(itemId, PityResult.Success);
                } else {
                    _tracker.Record(itemId, PityResult.Failure);
                    if (_tracker.GetFailures(itemId) >= config.Threshold) {
                        // Guarantee the drop
                        ForceDrop(workingSet, itemId, config);
                        _tracker.Record(itemId, PityResult.Success);
                    }
                }
            }
        }

        private void ForceDrop(LootWorkingSet<T> workingSet, int itemId, PityConfig config) {
            // Try to find the entry in the table to respect its quantity roll and metadata
            foreach (var we in workingSet.WeightedEntries) {
                if (we.Entry.Item != null && _idExtractor(we.Entry.Item) == itemId) {
                    workingSet.AddResult(we.Entry, we.Index, 1f); // 1f roll value for "guaranteed"
                    return;
                }
            }

            // Fallback: add the item directly if it's not in the current table
            workingSet.AddResult(config.Item, 1);
        }

        /// <summary>
        /// Gets the current failure count for an item.
        /// </summary>
        public int GetFailures(T item) => _tracker.GetFailures(_idExtractor(item));

        /// <summary>
        /// Resets the failure counter for an item.
        /// </summary>
        public void Reset(T item) => _tracker.Record(_idExtractor(item), PityResult.Success);

        /// <summary>
        /// Resets all failure counters.
        /// </summary>
        public void ResetAll() => _tracker.ResetAll();
    }
}
