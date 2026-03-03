namespace NS.UnifiedLoot
{
    /// <summary>
    /// A single step in the loot pipeline. Strategies can generate, filter, or modify loot results.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public interface ILootStrategy<T>
    {
        /// <summary>
        /// Processes the current loot working set. Strategies can add, remove, or modify results.
        /// </summary>
        /// <param name="workingSet">The current state of the loot roll, including results and table reference.</param>
        /// <param name="context">Contextual data for this roll (player stats, etc.).</param>
        void Process(LootWorkingSet<T> workingSet, LootContext context);
    }
}
