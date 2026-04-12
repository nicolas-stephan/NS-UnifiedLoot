using System;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Modifies the quantity of all results.
    /// </summary>
    public class ModifyQuantityStrategy<T> : ILootResultModifierStrategy<T> {
        private readonly Func<int, Context, int> _modifier;

        /// <summary>
        /// Returns the modifier function used by this strategy.
        /// </summary>
        public Func<int, Context, int> Modifier => _modifier;

        /// <summary>
        /// Creates a quantity modifier.
        /// </summary>
        /// <param name="modifier">Function that takes the current quantity and context, returns new quantity.</param>
        public ModifyQuantityStrategy(Func<int, Context, int> modifier) => _modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));

        /// <summary>
        /// Creates a simple multiplier.
        /// </summary>
        public static ModifyQuantityStrategy<T> Multiplier(float multiplier) => new((qty, _) => (int)(qty * multiplier));

        /// <summary>
        /// Creates a multiplier that reads from context.
        /// </summary>
        public static ModifyQuantityStrategy<T> MultiplierFromContext(Key<float> key, float defaultMultiplier = 1f)
            => new((qty, ctx) => (int)(qty * ctx.GetOrDefault(key, defaultMultiplier)));

        /// <summary>
        /// Creates a multiplier that reads from context.
        /// </summary>
        public static ModifyQuantityStrategy<T> MultiplierFromContext(Key<int> key, int defaultMultiplier = 1)
            => new((qty, ctx) => qty * ctx.GetOrDefault(key, defaultMultiplier));

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            for (var i = 0; i < workingSet.Results.Count; i++) {
                var result = workingSet.Results[i];
                workingSet.Results[i] = new LootResult<T>(result.Item, Math.Max(1, _modifier(result.Quantity, context)), result.Metadata);
            }
        }

        public void OnPreview(LootTablePreview<T> preview, Context context) {
            foreach (var entry in preview.Entries) {
                var min = _modifier(entry.ModifiedQuantity.Min, context);
                var max = _modifier(entry.ModifiedQuantity.Max, context);
                entry.ModifiedQuantity = new IntRange(Math.Max(1, min), Math.Max(1, max));
            }
        }
    }
}
