using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Tracks items that have already dropped and prevents duplicates.
    /// Useful for one-time drops (e.g., unique quest items, first-time boss kills).
    /// </summary>
    public class UniqueDropStrategy<T> : ILootResultModifierStrategy<T>, IResettable {
        private readonly HashSet<T> _droppedItems;

        public UniqueDropStrategy(IEqualityComparer<T>? comparer = null)
            => _droppedItems = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            for (var i = workingSet.Results.Count - 1; i >= 0; i--) {
                var item = workingSet.Results[i].Item;

                if (!_droppedItems.Add(item))
                    workingSet.Results.RemoveAt(i);
            }
        }

        /// <summary>
        /// Resets the tracked drops for a specific item.
        /// </summary>
        public void Reset(T item) => _droppedItems.Remove(item);

        /// <summary>
        /// Resets all tracked drops.
        /// </summary>
        public void ResetAll() => _droppedItems.Clear();

        /// <summary>
        /// Manually marks an item as already dropped.
        /// </summary>
        public void MarkAsDropped(T item) => _droppedItems.Add(item);

        /// <summary>
        /// Checks if an item has already dropped.
        /// </summary>
        public bool HasDropped(T item) => _droppedItems.Contains(item);
    }
}