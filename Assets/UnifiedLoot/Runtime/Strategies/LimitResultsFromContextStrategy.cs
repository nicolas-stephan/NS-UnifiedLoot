
namespace NS.UnifiedLoot {
    /// <summary>
    /// Limits results based on a context value.
    /// </summary>
    public class LimitResultsFromContextStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly Key<int> _key;
        private readonly int _defaultMax;

        public LimitResultsFromContextStrategy(Key<int> key, int defaultMax = int.MaxValue) {
            _key = key;
            _defaultMax = defaultMax;
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            var maxResults = context.GetOrDefault(_key, _defaultMax);
            if (workingSet.Results.Count > maxResults)
                workingSet.Results.RemoveRange(maxResults, workingSet.Results.Count - maxResults);
        }
    }
}
