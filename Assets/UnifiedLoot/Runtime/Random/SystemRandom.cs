namespace NS.UnifiedLoot {
    /// <summary>
    /// Default implementation using System.Random.
    /// </summary>
    public class SystemRandom : IRandom {
        private readonly System.Random _random;

        public SystemRandom() => _random = new System.Random();

        public SystemRandom(int seed) => _random = new System.Random(seed);

        public float Value => (float)_random.NextDouble();

        public int Range(int min, int maxExclusive) => _random.Next(min, maxExclusive);

        public float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);
    }
}
