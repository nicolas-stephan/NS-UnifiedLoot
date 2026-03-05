namespace NS.UnifiedLoot {
    /// <summary>
    /// Ensures at least one item drops if the current results are empty.
    /// Useful as a fallback at the end of a pipeline.
    /// </summary>
    public class GuaranteedDropStrategy<T> : ILootGeneratorStrategy<T> {
        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            if (workingSet.Results.Count > 0)
                return;

            workingSet.TryRollOneResult();
        }
    }
}