using System;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// Filters results based on a predicate that has access to context.
    /// </summary>
    public class FilterByContextStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly Func<LootResult<T>, Context, bool> _predicate;

        /// <summary>
        /// Creates a context-aware filter.
        /// </summary>
        /// <param name="predicate">Returns true to keep the result, false to remove it.</param>
        public FilterByContextStrategy(Func<LootResult<T>, Context, bool> predicate)
            => _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            for (var i = workingSet.Results.Count - 1; i >= 0; i--)
                if (!_predicate(workingSet.Results[i], context))
                    workingSet.Results.RemoveAt(i);
        }
    }
}