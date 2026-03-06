using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Preview;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies
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
        void Process(LootWorkingSet<T> workingSet, Context context);
    }

    /// <summary>
    /// A strategy that modifies the loot table (weights, entries) before any items are rolled.
    /// These strategies are executed during the "dry-run" preview.
    /// </summary>
    public interface ILootTableModifierStrategy<T> : ILootStrategy<T> { }

    /// <summary>
    /// A strategy that selects items from the table and adds them as results.
    /// These strategies are typically skipped during the "dry-run" preview.
    /// </summary>
    public interface ILootGeneratorStrategy<T> : ILootStrategy<T> { }

    /// <summary>
    /// A strategy that modifies loot results after they have been generated.
    /// </summary>
    public interface ILootResultModifierStrategy<T> : ILootStrategy<T>
    {
        /// <summary>
        /// Allows the strategy to reflect its changes in a table preview.
        /// For example, a quantity modifier can update the preview quantities of all entries.
        /// </summary>
        void OnPreview(LootTablePreview<T> preview, Context context) { }
    }
}
