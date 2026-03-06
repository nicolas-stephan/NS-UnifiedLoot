using System;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies {
    /// <summary>
    /// Scales the weights of weighted entries before selection strategies run.
    /// Must be placed <em>before</em> <see cref="WeightedRandomStrategy{T}"/> in the pipeline.
    /// </summary>
    public class ModifyWeightStrategy<T> : ILootTableModifierStrategy<T> {
        private readonly Func<WeightedEntry<T>, Context, float> _modifier;

        /// <summary>
        /// Creates a weight modifier with a custom delegate.
        /// </summary>
        /// <param name="modifier">
        /// Returns the new weight for an entry. Negative values are clamped to zero.
        /// </param>
        public ModifyWeightStrategy(Func<WeightedEntry<T>, Context, float> modifier)
            => _modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));

        /// <summary>
        /// Multiplies every entry's weight by a fixed multiplier.
        /// </summary>
        public static ModifyWeightStrategy<T> Multiplier(float multiplier)
            => new((entry, _) => entry.Weight * multiplier);

        /// <summary>
        /// Reads a float multiplier from the context and applies it to every entry's weight.
        /// Falls back to <paramref name="defaultMultiplier"/> when the key is absent.
        /// </summary>
        public static ModifyWeightStrategy<T> MultiplierFromContext(Key<float> key, float defaultMultiplier = 1f)
            => new((entry, ctx) => entry.Weight * ctx.GetOrDefault(key, defaultMultiplier));

        /// <summary>
        /// Multiplies weights of entries whose context value falls within <paramref name="range"/>.
        /// Entries outside the range are left unchanged.
        /// </summary>
        public static ModifyWeightStrategy<T> ScaleByContextRange(Key<int> key, IntRange range, float multiplier)
            => new((entry, ctx) => {
                var value = ctx.GetOrDefault(key);
                return range.Contains(value) ? entry.Weight * multiplier : entry.Weight;
            });

        public void Process(LootWorkingSet<T> workingSet, Context context) {
            if (workingSet.WeightedEntries.Count == 0)
                return;

            var cumulative = 0f;
            for (var i = 0; i < workingSet.WeightedEntries.Count; i++) {
                var we = workingSet.WeightedEntries[i];
                var newWeight = Math.Max(0f, _modifier(we, context));
                cumulative += newWeight;
                workingSet.WeightedEntries[i] = new WeightedEntry<T> {
                    Entry = we.Entry,
                    Index = we.Index,
                    Weight = newWeight,
                    CumulativeWeight = cumulative
                };
            }

            workingSet.TotalWeight = cumulative;
        }
    }
}
