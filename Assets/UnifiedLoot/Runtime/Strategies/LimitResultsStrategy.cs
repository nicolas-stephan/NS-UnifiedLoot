namespace NS.UnifiedLoot {
    /// <summary>
    /// Limits the number of results to a maximum count.
    /// </summary>
    public class LimitResultsStrategy<T> : ILootStrategy<T> {
        private readonly int _maxResults;

        public LimitResultsStrategy(int maxResults) => _maxResults = maxResults;

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            if (workingSet.Results.Count > _maxResults)
                workingSet.Results.RemoveRange(_maxResults, workingSet.Results.Count - _maxResults);
        }
    }

}