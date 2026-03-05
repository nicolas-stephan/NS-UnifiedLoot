using System;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Filters results based on a simple predicate.
    /// </summary>
    public class FilterStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly Func<LootResult<T>, bool> _predicate;

        /// <summary>
        /// Creates a simple filter.
        /// </summary>
        /// <param name="predicate">Returns true to keep the result, false to remove it.</param>
        public FilterStrategy(Func<LootResult<T>, bool> predicate)
            => _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            for (var i = workingSet.Results.Count - 1; i >= 0; i--)
                if (!_predicate(workingSet.Results[i]))
                    workingSet.Results.RemoveAt(i);
        }
    }
}
