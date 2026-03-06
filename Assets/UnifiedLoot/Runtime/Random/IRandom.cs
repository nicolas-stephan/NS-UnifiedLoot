namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Random {
    /// <summary>
    /// Abstraction for random number generation, allowing for deterministic/seeded randomness.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a random float between 0 (inclusive) and 1 (exclusive).
        /// </summary>
        float Value { get; }

        /// <summary>
        /// Returns a random integer in the range [min, max).
        /// </summary>
        int Range(int min, int maxExclusive);

        /// <summary>
        /// Returns a random float in the range [min, max).
        /// </summary>
        float Range(float min, float max);
    }
}
