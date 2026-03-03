using System;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Allows rolling on additional tables from the results.
    /// Useful for nested loot pools (e.g., a "weapon" entry that rolls on a weapon-specific table).
    /// For each matched result, the nested pipeline is executed once per quantity unit.
    /// </summary>
    public class NestedTableStrategy<T> : ILootStrategy<T> {
        private readonly Func<T, ILootTable<T>> _tableResolver;
        private readonly LootPipeline<T> _pipeline;

        /// <summary>
        /// Creates a nested table strategy.
        /// </summary>
        /// <param name="tableResolver">Function that returns a nested table for an item, or null if not nested.</param>
        /// <param name="pipeline">The pipeline to use for nested rolls. If null, uses a simple weighted random.</param>
        public NestedTableStrategy(Func<T, ILootTable<T>> tableResolver, LootPipeline<T>? pipeline = null) {
            _tableResolver = tableResolver ?? throw new ArgumentNullException(nameof(tableResolver));
            _pipeline = pipeline ?? new LootPipeline<T>().AddStrategy(new WeightedRandomStrategy<T>());
        }

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            var toAdd = new List<LootResult<T>>();
            var toRemove = new List<int>();

            for (var i = 0; i < workingSet.Results.Count; i++) {
                var result = workingSet.Results[i];
                var nestedTable = _tableResolver(result.Item);

                if (nestedTable == null)
                    continue;

                toRemove.Add(i);

                for (var q = 0; q < result.Quantity; q++)
                    toAdd.AddRange(_pipeline.Execute(nestedTable, context, workingSet.Random));
            }

            // Remove in reverse order to maintain indices
            for (var i = toRemove.Count - 1; i >= 0; i--)
                workingSet.Results.RemoveAt(toRemove[i]);

            workingSet.Results.AddRange(toAdd);
        }
    }
}
