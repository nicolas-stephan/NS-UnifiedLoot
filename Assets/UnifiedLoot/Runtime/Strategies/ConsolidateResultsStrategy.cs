using System.Collections.Generic;
using UnityEngine.Pool;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Consolidates duplicate items by combining their quantities.
    /// Useful after strategies that might produce multiple rolls of the same item.
    /// </summary>
    public class ConsolidateResultsStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly IEqualityComparer<T> _comparer;

        public ConsolidateResultsStrategy(IEqualityComparer<T>? comparer = null) => _comparer = comparer ?? EqualityComparer<T>.Default;

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            if (workingSet.Results.Count <= 1)
                return;

            var usePool = _comparer == EqualityComparer<T>.Default;
            var consolidated = usePool
                ? GenericPool<Dictionary<T, LootResult<T>>>.Get()
                : new Dictionary<T, LootResult<T>>(_comparer);

            consolidated.Clear();
            consolidated.EnsureCapacity(workingSet.Results.Count);

            try {
                foreach (var result in workingSet.Results) {
                    if (consolidated.TryGetValue(result.Item, out var existing))
                        consolidated[result.Item] = new LootResult<T>(existing.Item, existing.Quantity + result.Quantity, existing.Metadata);
                    else
                        consolidated[result.Item] = result;
                }

                workingSet.Results.Clear();
                workingSet.Results.Capacity = consolidated.Count;
                foreach (var kvp in consolidated)
                    workingSet.Results.Add(kvp.Value);
            } finally {
                if (usePool)
                    GenericPool<Dictionary<T, LootResult<T>>>.Release(consolidated);
            }
        }
    }
}
