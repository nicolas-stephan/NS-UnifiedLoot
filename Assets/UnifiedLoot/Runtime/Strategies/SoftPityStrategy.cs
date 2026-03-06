using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Pity;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// A probabilistic pity system whose bonus-drop chance increases linearly after each
    /// consecutive failure, guaranteeing a drop once the hard-pity threshold is reached.
    ///
    /// <para>
    /// Placement: add <em>after</em> the main selection strategy so it only activates when
    /// the main strategy produced no results.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Probability schedule (linear ramp):
    /// <list type="bullet">
    ///   <item>failures &lt;= <c>softPityStart</c>: 0 %</item>
    ///   <item>failures in (<c>softPityStart</c>, <c>hardPityAt</c>): linearly rising to 100 %</item>
    ///   <item>failures &gt;= <c>hardPityAt</c>: 100 % (guaranteed)</item>
    /// </list>
    /// </remarks>
    public class SoftPityStrategy<T> : ILootTableModifierStrategy<T>, IResettable {
        private readonly int _softPityStart;
        private readonly int _hardPityAt;
        private readonly int? _groupKey;
        private readonly IPityTracker _tracker;

        /// <summary>
        /// Creates a soft-pity strategy.
        /// </summary>
        /// <param name="softPityStart">
        /// The number of failures before the bonus chance begins to climb. Below this, the bonus is 0 %.
        /// </param>
        /// <param name="hardPityAt">
        /// Number of failures that guarantees a bonus drop (100 %). Must be &gt; <paramref name="softPityStart"/>.
        /// </param>
        /// <param name="groupKey">
        /// When set, all tables rolling through this strategy share the same failure counter
        /// (identified by <paramref name="groupKey"/>). When <c>null</c>, each table tracks
        /// its own counter independently.
        /// </param>
        /// <param name="tracker">The tracker to use. When <c>null</c>, creates its own tracker instance.</param>
        public SoftPityStrategy(int softPityStart, int hardPityAt, int? groupKey = null, IPityTracker? tracker = null) {
            _softPityStart = softPityStart;
            _hardPityAt = hardPityAt;
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
            failures++;

            var chance = ComputeChance(failures);

            if (chance <= 0f) {
                _tracker.RecordFailure(key);
                return;
            }

            var wasGuaranteed = failures >= _hardPityAt;

            if (wasGuaranteed || workingSet.Random.Value < chance) {
                workingSet.TryRollOneResult();
                _tracker.RecordSuccess(key);
            } else {
                _tracker.RecordFailure(key);
            }
        }

        private float ComputeChance(int failures) {
            if (failures >= _hardPityAt)
                return 1f;
            if (failures <= _softPityStart)
                return 0f;
            return (float)(failures - _softPityStart) / (_hardPityAt - _softPityStart);
        }

        /// <summary>
        /// Resets the failure counter for a specific key.
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