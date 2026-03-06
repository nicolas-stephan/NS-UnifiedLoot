using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// Rolls each entry independently against its weight as a drop chance (0-1 or 0-100).
    /// Unlike weighted random, entries don't compete - each is rolled separately.
    /// </summary>
    public class DropChanceStrategy<T> : ILootGeneratorStrategy<T> {
        private readonly float _weightScale;

        /// <summary>
        /// Creates a drop chance strategy.
        /// </summary>
        /// <param name="weightAsPercent">If true, treats weight as 0-100 percent. If false, treats weight as 0-1 probability.</param>
        public DropChanceStrategy(bool weightAsPercent = false) => _weightScale = weightAsPercent ? 0.01f : 1f;

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            foreach (var weighted in workingSet.WeightedEntries) {
                var dropChance = weighted.Weight * _weightScale;
                var roll = workingSet.Random.Value;

                if (roll < dropChance)
                    workingSet.AddResult(weighted.Entry, weighted.Index, roll);
            }
        }
    }
}