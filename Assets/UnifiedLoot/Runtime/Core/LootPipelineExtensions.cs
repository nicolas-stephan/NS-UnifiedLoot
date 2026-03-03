using System.Collections.Generic;

using UnityEngine.Pool;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Extension methods for LootPipeline.
    /// </summary>
    public static class LootPipelineExtensions {

        /// <summary>
        /// Executes the pipeline and converts results using a factory.
        /// </summary>
        /// <typeparam name="TDef">The definition type in the loot table.</typeparam>
        /// <typeparam name="TInstance">The instance type to create.</typeparam>
        /// <param name="pipeline">The loot pipeline.</param>
        /// <param name="table">The loot table to roll against.</param>
        /// <param name="factory">The factory to convert definitions to instances.</param>
        /// <param name="results">The list to append results to.</param>
        /// <param name="context">Optional context data.</param>
        /// <param name="random">Optional random override.</param>
        /// <returns>List of built loot results with instances.</returns>
        public static void ExecuteAndBuild<TDef, TInstance>(
            this LootPipeline<TDef> pipeline,
            ILootTable<TDef> table,
            ILootFactory<TDef, TInstance> factory,
            List<BuiltLootResult<TDef, TInstance>> results,
            LootContext? context = null,
            IRandom? random = null
        ) {
            context ??= LootPipeline.EmptyContext;
            random ??= pipeline.DefaultRandom;

            var rawResults = ListPool<LootResult<TDef>>.Get();
            try {
                pipeline.Execute(table, rawResults, context, random);

                foreach (var result in rawResults) {
                    var instance = factory.Create(result.Item, context, random);
                    results.Add(new BuiltLootResult<TDef, TInstance> {
                        Definition = result.Item,
                        Instance = instance,
                        Quantity = result.Quantity,
                        Metadata = result.Metadata
                    });
                }
            } finally {
                ListPool<LootResult<TDef>>.Release(rawResults);
            }
        }
    }

    /// <summary>
    /// Result of a loot roll that has been built into an instance.
    /// Contains both the original definition and the created instance.
    /// </summary>
    /// <typeparam name="TDefinition">The loot definition type.</typeparam>
    /// <typeparam name="TInstance">The created instance type.</typeparam>
    public readonly struct BuiltLootResult<TDefinition, TInstance> {
        /// <summary>
        /// The original loot definition that was rolled.
        /// </summary>
        public TDefinition Definition { get; init; }

        /// <summary>
        /// The created item instance.
        /// </summary>
        public TInstance Instance { get; init; }

        /// <summary>
        /// The quantity rolled.
        /// </summary>
        public int Quantity { get; init; }

        /// <summary>
        /// Metadata about the roll.
        /// </summary>
        public LootMetadata Metadata { get; init; }

        public override string ToString() => Quantity == 1 ? $"{Instance}" : $"{Instance} x{Quantity}";
    }
}