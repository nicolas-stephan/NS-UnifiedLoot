using System.Collections.Generic;
using System.Threading;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Represents a collection of loot entries that can be rolled against.
    /// </summary>
    /// <typeparam name="T">The type of item in the table.</typeparam>
    public interface ILootTable<out T> : IEnumerable<ILootEntry<T>> {
        int Id { get; }

        /// <summary>
        /// The number of entries in this table.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the entry at the specified index.
        /// </summary>
        ILootEntry<T> this[int index] { get; }
    }

    /// <summary>
    /// Helper for generating unique table IDs.
    /// </summary>
    public static class LootTableIdGenerator {
        private static int _nextId;
        public static int GetNextId() => Interlocked.Increment(ref _nextId);
    }
}
