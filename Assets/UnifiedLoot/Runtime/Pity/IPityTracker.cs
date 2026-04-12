namespace NS.UnifiedLoot {
    public enum PityResult {
        Success,
        Failure
    }

    /// <summary>
    /// Tracks failure/success counts for a pity system.
    /// Can be used standalone or within the loot pipeline.
    /// </summary>
    public interface IPityTracker {
        /// <summary>
        /// Gets the number of consecutive failures for a specific key.
        /// </summary>
        int GetFailures(int key);

        /// <summary>
        /// Increments the failure count for a specific key.
        /// </summary>
        void Record(int key, PityResult pityResult);

        /// <summary>
        /// Resets all counters.
        /// </summary>
        void ResetAll();
    }
}
