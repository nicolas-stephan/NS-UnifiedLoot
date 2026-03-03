namespace NS.UnifiedLoot
{
    /// <summary>
    /// Implementation using Unity's random. Not seedable per-instance.
    /// </summary>
    public class UnityRandom : IRandom
    {
        public static readonly UnityRandom Instance = new();

        public float Value => UnityEngine.Random.value;

        public int Range(int min, int maxExclusive) => UnityEngine.Random.Range(min, maxExclusive);

        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);
    }
}
