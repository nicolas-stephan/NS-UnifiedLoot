using System;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Modifies the quantity of all results.
    /// </summary>
    public class ModifyQuantityStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly Func<int, LootContext, int> _modifier;

        /// <summary>
        /// Returns the modifier function used by this strategy.
        /// </summary>
        public Func<int, LootContext, int> Modifier => _modifier;

        /// <summary>
        /// Creates a quantity modifier.
        /// </summary>
        /// <param name="modifier">Function that takes the current quantity and context, returns new quantity.</param>
        public ModifyQuantityStrategy(Func<int, LootContext, int> modifier) => _modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));

        /// <summary>
        /// Creates a simple multiplier.
        /// </summary>
        public static ModifyQuantityStrategy<T> Multiplier(float multiplier) => new((qty, _) => (int)(qty * multiplier));

        /// <summary>
        /// Creates a multiplier that reads from context.
        /// </summary>
        public static ModifyQuantityStrategy<T> MultiplierFromContext(ContextKey<float> key, float defaultMultiplier = 1f)
            => new((qty, ctx) => (int)(qty * ctx.GetOrDefault(key, defaultMultiplier)));

        public void Process(LootWorkingSet<T> workingSet, LootContext context) {
            for (var i = 0; i < workingSet.Results.Count; i++) {
                var result = workingSet.Results[i];
                workingSet.Results[i] = new LootResult<T>(result.Item, Math.Max(1, _modifier(result.Quantity, context)), result.Metadata);
            }
        }

        public void OnPreview(LootTablePreview<T> preview, LootContext context) {
            foreach (var entry in preview.Entries) {
                var min = _modifier(entry.ModifiedQuantity.Min, context);
                var max = _modifier(entry.ModifiedQuantity.Max, context);
                entry.ModifiedQuantity = new IntRange(Math.Max(1, min), Math.Max(1, max));
            }
        }
    }
}