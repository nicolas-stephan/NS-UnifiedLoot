using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Core {
    /// <summary>
    /// Factory interface for converting loot definitions into actual item instances.
    /// Users implement this to define how their items are created from loot results.
    /// </summary>
    /// <typeparam name="TDefinition">The loot definition type (what the table contains).</typeparam>
    /// <typeparam name="TInstance">The item instance type (what the user actually receives).</typeparam>
    public interface ILootFactory<in TDefinition, out TInstance>
    {
        /// <summary>
        /// Creates an item instance from a loot definition.
        /// </summary>
        /// <param name="definition">The loot definition/template.</param>
        /// <param name="context">The loot context (for accessing player stats, etc.).</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>The created item instance.</returns>
        TInstance Create(TDefinition definition, Context context, IRandom random);
    }
}
