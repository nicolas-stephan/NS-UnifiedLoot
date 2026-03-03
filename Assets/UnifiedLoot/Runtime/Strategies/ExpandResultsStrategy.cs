using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Replaces individual results with an expanded collection — useful for "drop sets"
    /// where one logical item resolves into a group of concrete items.
    ///
    /// <para>
    /// For each result, the <c>expander</c> delegate is called. If it returns a non-null
    /// collection, the original result is removed and replaced with one <see cref="LootResult{T}"/>
    /// per expanded item (quantity defaults to 1 unless an optional <c>quantityResolver</c>
    /// is provided).  If the delegate returns <c>null</c>, the result passes through unchanged.
    /// </para>
    /// </summary>
    public class ExpandResultsStrategy<T> : ILootStrategy<T> {
        private readonly Func<T, IEnumerable<T>?> _expander;
        private readonly Func<T, int>? _quantityResolver;

        /// <summary>
        /// Creates an expand-results strategy.
        /// </summary>
        /// <param name="expander">
        /// Given an item, return a replacement collection or <c>null</c> to leave the item unchanged.
        /// </param>
        /// <param name="quantityResolver">
        /// Optional: given an expanded item, return the quantity. Defaults to 1 when <c>null</c>.
        /// </param>
        public ExpandResultsStrategy(Func<T, IEnumerable<T>?> expander, Func<T, int>? quantityResolver = null) {
            _expander = expander ?? throw new ArgumentNullException(nameof(expander));
            _quantityResolver = quantityResolver;
        }

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            var expanded = ListPool<LootResult<T>>.Get();
            var anyExpanded = false;

            try {
                foreach (var result in workingSet.Results) {
                    var expansion = _expander(result.Item);
                    if (expansion == null) {
                        expanded.Add(result);
                        continue;
                    }

                    anyExpanded = true;
                    foreach (var item in expansion) {
                        var qty = _quantityResolver?.Invoke(item) ?? 1;
                        expanded.Add(new LootResult<T>(item, qty, result.Metadata));
                    }
                }

                if (!anyExpanded)
                    return;

                workingSet.Results.Clear();
                workingSet.Results.AddRange(expanded);
            } finally {
                ListPool<LootResult<T>>.Release(expanded);
            }
        }
    }
}