
namespace NS.UnifiedLoot {
    /// <summary>
    /// Adds bonus rolls based on context values (like luck or magic find).
    /// </summary>
    public class BonusRollStrategy<T> : ILootGeneratorStrategy<T> {
        private readonly Key<float> _bonusChanceKey;
        private readonly float _defaultChance;

        /// <summary>
        /// Creates a bonus roll strategy.
        /// </summary>
        /// <param name="bonusChanceKey">Context key containing the bonus chance (0-1).</param>
        /// <param name="defaultChance">Default chance if key not in context.</param>
        public BonusRollStrategy(Key<float> bonusChanceKey, float defaultChance = 0f) {
            _bonusChanceKey = bonusChanceKey;
            _defaultChance = defaultChance;
        }

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            var bonusChance = context.GetOrDefault(_bonusChanceKey, _defaultChance);

            if (
                bonusChance <= 0f ||
                workingSet.WeightedEntries.Count == 0 ||
                workingSet.TotalWeight <= 0f ||
                workingSet.Random.Value >= bonusChance
            )
                return;

            workingSet.TryRollOneResult();
        }
    }
}
