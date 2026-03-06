namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Pity {
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
        void RecordFailure(int key);

        /// <summary>
        /// Resets the failure count for a specific key (usually on success).
        /// </summary>
        void RecordSuccess(int key);

        /// <summary>
        /// Resets all counters.
        /// </summary>
        void ResetAll();
    }
}
