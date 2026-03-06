using System;
using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Pity;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// A probabilistic pity strategy for specific items.
    /// The chance for a bonus drop increases linearly after each failure.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public class SoftItemPityStrategy<T> : ILootStrategy<T>, IResettable {
        private readonly IPityTracker _tracker;
        private readonly Func<T, int> _idExtractor;
        private readonly Dictionary<int, SoftPityConfig> _configs = new();

        private class SoftPityConfig {
            public T Item;
            public int SoftPityStart;
            public int HardPityAt;
        }

        public SoftItemPityStrategy(Func<T, int> idExtractor, IPityTracker? tracker = null) {
            _idExtractor = idExtractor;
            _tracker = tracker ?? new PityTracker();
        }

        /// <summary>
        /// Configures soft pity for a specific item.
        /// </summary>
        /// <param name="item">The item to track.</param>
        /// <param name="softPityStart">Number of failures before the bonus chance begins to climb.</param>
        /// <param name="hardPityAt">Number of failures that guarantees a drop (100 %).</param>
        public void AddTrackedItem(T item, int softPityStart, int hardPityAt) {
            var id = _idExtractor(item);
            _configs[id] = new SoftPityConfig {
                Item = item,
                SoftPityStart = softPityStart,
                HardPityAt = hardPityAt
            };
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            var droppedIds = new HashSet<int>();
            foreach (var result in workingSet.Results) {
                if (result.Item != null) {
                    droppedIds.Add(_idExtractor(result.Item));
                }
            }

            foreach (var kvp in _configs) {
                var itemId = kvp.Key;
                var config = kvp.Value;

                if (droppedIds.Contains(itemId)) {
                    _tracker.RecordSuccess(itemId);
                } else {
                    _tracker.RecordFailure(itemId);
                    var failures = _tracker.GetFailures(itemId);
                    var chance = ComputeChance(failures, config);

                    if (chance > 0f && (failures >= config.HardPityAt || workingSet.Random.Value < chance)) {
                        ForceDrop(workingSet, itemId, config);
                        _tracker.RecordSuccess(itemId);
                    }
                }
            }
        }

        private float ComputeChance(int failures, SoftPityConfig config) {
            if (failures >= config.HardPityAt) return 1f;
            if (failures <= config.SoftPityStart) return 0f;
            return (float)(failures - config.SoftPityStart) / (config.HardPityAt - config.SoftPityStart);
        }

        private void ForceDrop(LootWorkingSet<T> workingSet, int itemId, SoftPityConfig config) {
            foreach (var we in workingSet.WeightedEntries) {
                if (we.Entry.Item != null && _idExtractor(we.Entry.Item) == itemId) {
                    workingSet.AddResult(we.Entry, we.Index, 1f);
                    return;
                }
            }
            workingSet.AddResult(config.Item, 1);
        }

        public int GetFailures(T item) => _tracker.GetFailures(_idExtractor(item));
        public void Reset(T item) => _tracker.RecordSuccess(_idExtractor(item));
        public void ResetAll() => _tracker.ResetAll();
    }
}
