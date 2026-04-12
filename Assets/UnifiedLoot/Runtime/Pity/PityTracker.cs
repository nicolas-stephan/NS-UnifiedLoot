using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Default in-memory implementation of <see cref="IPityTracker"/>.
    /// Stores failure counts in a dictionary.
    /// </summary>
    public class PityTracker : IPityTracker {
        private readonly Dictionary<int, int> _failureCounts = new();

        public int GetFailures(int key) => _failureCounts.GetValueOrDefault(key, 0);

        public void Record(int key, PityResult pityResult) {
            if (pityResult == PityResult.Success)
                _failureCounts.Remove(key);
            else
                _failureCounts[key] = GetFailures(key) + 1;
        }

        public void ResetAll() => _failureCounts.Clear();
    }
}
