using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Observes completed loot rolls. Register via <see cref="LootPipeline{T}.AddObserver"/>.
    /// Called after all strategies have finished processing.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public interface ILootObserver<T> {
        /// <summary>
        /// Invoked after every successful pipeline execution.
        /// </summary>
        /// <param name="table">The table that was rolled against.</param>
        /// <param name="results">The final results after all strategies ran.</param>
        /// <param name="context">The context used for this roll.</param>
        void OnRollComplete(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, LootContext context);
    }
}
