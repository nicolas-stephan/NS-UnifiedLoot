using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using UnityEngine.Pool;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// Generates loot results using weighted random selection.
    /// This is typically the first strategy in a pipeline.
    /// </summary>
    public class WeightedRandomStrategy<T> : ILootGeneratorStrategy<T> {
        private readonly int _rollCount;
        private readonly bool _allowDuplicates;

        /// <summary>
        /// Creates a weighted random roller.
        /// </summary>
        /// <param name="rollCount">Number of times to roll. Each roll can produce one result.</param>
        /// <param name="allowDuplicates">Whether the same entry can be selected multiple times.</param>
        public WeightedRandomStrategy(int rollCount = 1, bool allowDuplicates = true) {
            _rollCount = rollCount;
            _allowDuplicates = allowDuplicates;
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            if (workingSet.WeightedEntries.Count == 0 || workingSet.TotalWeight <= 0f)
                return;

            if (_allowDuplicates) {
                for (var i = 0; i < _rollCount; i++) {
                    var roll = workingSet.Random.Range(0f, workingSet.TotalWeight);
                    var entry = SelectEntry(workingSet, roll);
                    if (!entry.HasValue)
                        continue;
                    var (selected, index) = entry.Value;
                    workingSet.AddResult(selected, index, roll / workingSet.TotalWeight);
                }
            } else {
                var excluded = HashSetPool<int>.Get();
                try {
                    for (var i = 0; i < _rollCount && excluded.Count < workingSet.WeightedEntries.Count; i++) {
                        var (selected, index, rollValue) = SelectExcluding(workingSet, excluded);
                        if (selected == null)
                            break;
                        excluded.Add(index);
                        workingSet.AddResult(selected, index, rollValue);
                    }
                } finally {
                    HashSetPool<int>.Release(excluded);
                }
            }
        }

        private static (ILootEntry<T> entry, int index)? SelectEntry(LootWorkingSet<T> workingSet, float roll) {
            var low = 0;
            var high = workingSet.WeightedEntries.Count - 1;
            var selectedIndex = high;

            while (low <= high) {
                var mid = low + (high - low) / 2;
                if (workingSet.WeightedEntries[mid].CumulativeWeight >= roll) {
                    selectedIndex = mid;
                    high = mid - 1;
                } else {
                    low = mid + 1;
                }
            }

            var selected = workingSet.WeightedEntries[selectedIndex];
            return (selected.Entry, selected.Index);
        }

        private static (ILootEntry<T> entry, int index, float rollValue) SelectExcluding(LootWorkingSet<T> workingSet, HashSet<int> excluded) {
            var available = 0f;
            foreach (var we in workingSet.WeightedEntries)
                if (!excluded.Contains(we.Index))
                    available += we.Weight;

            var roll = workingSet.Random.Range(0f, available);
            var cumulated = 0f;
            foreach (var we in workingSet.WeightedEntries) {
                if (excluded.Contains(we.Index))
                    continue;
                cumulated += we.Weight;
                if (roll <= cumulated)
                    return (we.Entry, we.Index, available > 0f ? roll / available : 0f);
            }

            for (var i = workingSet.WeightedEntries.Count - 1; i >= 0; i--) {
                if (!excluded.Contains(workingSet.WeightedEntries[i].Index))
                    return (workingSet.WeightedEntries[i].Entry, workingSet.WeightedEntries[i].Index, 1f);
            }

            return default;
        }
    }
}