using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Pity;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// Implements a pity/mercy system that guarantees a drop after N failures.
    /// Tracks state per-table (or per shared group) using integer keys.
    /// Implements <see cref="IResettable"/> so callers can reset all counters via the interface.
    /// </summary>
    public class PityStrategy<T> : ILootTableModifierStrategy<T>, IResettable {
        private readonly int _maxFailures;
        private readonly int? _groupKey;
        private readonly IPityTracker _tracker;

        /// <summary>
        /// Creates a pity strategy that tracks failures per table ID.
        /// </summary>
        /// <param name="maxFailures">After this many empty rolls, guarantee a drop.</param>
        /// <param name="tracker">The tracker to use. When <c>null</c>, creates its own tracker instance.</param>
        public PityStrategy(int maxFailures, IPityTracker? tracker = null) {
            _maxFailures = maxFailures;
            _tracker = tracker ?? new PityTracker();
        }

        /// <summary>
        /// Creates a pity strategy with a shared group key.
        /// All tables rolling through this strategy instance share the same failure counter
        /// when a group key is provided — useful for boss encounters with multiple drop tables.
        /// </summary>
        /// <param name="maxFailures">After this many empty rolls, guarantee a drop.</param>
        /// <param name="groupKey">Shared counter key. When set, all tables use this key instead of their own ID.</param>
        /// <param name="tracker">The tracker to use. When <c>null</c>, creates its own tracker instance.</param>
        public PityStrategy(int maxFailures, int groupKey, IPityTracker? tracker = null) {
            _maxFailures = maxFailures;
            _groupKey = groupKey;
            _tracker = tracker ?? new PityTracker();
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            var key = _groupKey ?? workingSet.SourceTable?.Id ?? 0;

            if (workingSet.Results.Count > 0) {
                _tracker.RecordSuccess(key);
                return;
            }

            var failures = _tracker.GetFailures(key);
            _tracker.RecordFailure(key);
            failures++;

            if (failures >= _maxFailures) {
                workingSet.TryRollOneResult();
                _tracker.RecordSuccess(key);
            }
        }

        /// <summary>
        /// Resets the failure counter for a specific key (table ID or group key).
        /// </summary>
        public void Reset(int key) => _tracker.RecordSuccess(key);

        /// <summary>
        /// Resets all failure counters.
        /// </summary>
        public void ResetAll() => _tracker.ResetAll();

        /// <summary>
        /// Gets the current failure count for a key.
        /// </summary>
        public int GetFailureCount(int key) => _tracker.GetFailures(key);
    }
}
