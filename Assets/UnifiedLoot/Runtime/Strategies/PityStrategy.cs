using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Implements a pity/mercy system that guarantees a drop after N failures.
    /// Tracks state per-table (or per shared group) using integer keys.
    /// Implements <see cref="IResettable"/> so callers can reset all counters via the interface.
    /// </summary>
    public class PityStrategy<T> : ILootStrategy<T>, IResettable {
        private readonly int _maxFailures;
        private readonly int? _groupKey;
        private readonly Dictionary<int, int> _failureCounts = new();

        /// <summary>
        /// Creates a pity strategy that tracks failures per table ID.
        /// </summary>
        /// <param name="maxFailures">After this many empty rolls, guarantee a drop.</param>
        public PityStrategy(int maxFailures) => _maxFailures = maxFailures;

        /// <summary>
        /// Creates a pity strategy with a shared group key.
        /// All tables rolling through this strategy instance share the same failure counter
        /// when a group key is provided — useful for boss encounters with multiple drop tables.
        /// </summary>
        /// <param name="maxFailures">After this many empty rolls, guarantee a drop.</param>
        /// <param name="groupKey">Shared counter key. When set, all tables use this key instead of their own ID.</param>
        public PityStrategy(int maxFailures, int groupKey) {
            _maxFailures = maxFailures;
            _groupKey = groupKey;
        }

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            var key = _groupKey ?? workingSet.SourceTable?.Id ?? 0;

            if (workingSet.Results.Count > 0) {
                _failureCounts[key] = 0;
                return;
            }

            var failures = _failureCounts.GetValueOrDefault(key, 0);
            failures++;

            if (failures >= _maxFailures) {
                workingSet.TryRollOneResult();
                _failureCounts[key] = 0;
            } else {
                _failureCounts[key] = failures;
            }
        }

        /// <summary>
        /// Resets the failure counter for a specific key (table ID or group key).
        /// </summary>
        public void Reset(int key) => _failureCounts.Remove(key);

        /// <summary>
        /// Resets all failure counters.
        /// </summary>
        public void ResetAll() => _failureCounts.Clear();

        /// <summary>
        /// Gets the current failure count for a key.
        /// </summary>
        public int GetFailureCount(int key) => _failureCounts.GetValueOrDefault(key, 0);
    }
}
