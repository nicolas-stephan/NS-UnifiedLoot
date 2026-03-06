using System.Collections.Generic;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Pity {
    /// <summary>
    /// Default in-memory implementation of <see cref="IPityTracker"/>.
    /// Stores failure counts in a dictionary.
    /// </summary>
    public class PityTracker : IPityTracker {
        private readonly Dictionary<int, int> _failureCounts = new();

        public int GetFailures(int key) => _failureCounts.TryGetValue(key, out var failures) ? failures : 0;

        public void RecordFailure(int key) {
            _failureCounts[key] = GetFailures(key) + 1;
        }

        public void RecordSuccess(int key) {
            _failureCounts.Remove(key);
        }

        public void ResetAll() => _failureCounts.Clear();
    }
}
