using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Generates loot results using weighted random selection.
    /// This is typically the first strategy in a pipeline.
    /// </summary>
    public class WeightedRandomStrategy<T> : ILootStrategy<T> {
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

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
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
                var excluded = new HashSet<int>();
                for (var i = 0; i < _rollCount && excluded.Count < workingSet.WeightedEntries.Count; i++) {
                    var (selected, index, rollValue) = SelectExcluding(workingSet, excluded);
                    excluded.Add(index);
                    workingSet.AddResult(selected, index, rollValue);
                }
            }
        }

        private static (ILootEntry<T> entry, int index)? SelectEntry(LootWorkingSet<T> workingSet, float roll) {
            foreach (var weighted in workingSet.WeightedEntries)
                if (roll <= weighted.CumulativeWeight)
                    return (weighted.Entry, weighted.Index);

            // Fallback to last entry (handles floating point edge cases)
            var last = workingSet.WeightedEntries[^1];
            return (last.Entry, last.Index);
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