using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using UnityEngine.Pool;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Core {
    internal static class LootPipeline {
        internal static readonly Context EmptyContext = new();
    }

    /// <summary>
    /// A composable pipeline of strategies that process loot rolls.
    /// The pipeline defines HOW loot is processed; the table defines WHAT can drop.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public class LootPipeline<T> {
        private readonly List<ILootStrategy<T>> _strategies = new();
        private readonly List<ILootObserver<T>> _observers = new();
        private IRandom _defaultRandom;
        private bool _collectMetadata = true;

        internal IRandom DefaultRandom => _defaultRandom;

        /// <summary>
        /// Read-only view of the strategies currently in the pipeline.
        /// Use <see cref="InsertStrategy"/> and <see cref="RemoveStrategy"/> to mutate.
        /// </summary>
        public IReadOnlyList<ILootStrategy<T>> Strategies => _strategies;

        /// <param name="defaultRandom">The default random to use if none is provided at execution time.</param>
        public LootPipeline(IRandom? defaultRandom = null) => _defaultRandom = defaultRandom ?? UnityRandom.Instance;

        /// <summary>
        /// Appends a strategy to the end of the pipeline.
        /// </summary>
        public LootPipeline<T> AddStrategy(ILootStrategy<T> strategy) {
            _strategies.Add(strategy);
            return this;
        }

        /// <summary>
        /// Inserts a strategy at the given index in the pipeline.
        /// </summary>
        public LootPipeline<T> InsertStrategy(int index, ILootStrategy<T> strategy) {
            _strategies.Insert(index, strategy);
            return this;
        }

        /// <summary>
        /// Removes the first occurrence of a strategy from the pipeline.
        /// </summary>
        /// <returns>This pipeline for chaining.</returns>
        public LootPipeline<T> RemoveStrategy(ILootStrategy<T> strategy) {
            _strategies.Remove(strategy);
            return this;
        }

        /// <summary>
        /// Sets the default random number generator.
        /// </summary>
        public LootPipeline<T> WithRandom(IRandom random) {
            _defaultRandom = random;
            return this;
        }

        /// <summary>
        /// Enables or disables metadata collection. Disabling improves performance.
        /// </summary>
        public LootPipeline<T> WithMetadata(bool collect) {
            _collectMetadata = collect;
            return this;
        }

        /// <summary>
        /// Registers an observer notified after every roll completes.
        /// </summary>
        public LootPipeline<T> AddObserver(ILootObserver<T> observer) {
            _observers.Add(observer);
            return this;
        }

        /// <summary>
        /// Removes a previously registered observer.
        /// </summary>
        public LootPipeline<T> RemoveObserver(ILootObserver<T> observer) {
            _observers.Remove(observer);
            return this;
        }


        /// <summary>
        /// Executes the pipeline and writes results to an existing list (reduces allocations).
        /// </summary>
        public void Execute(ILootTable<T> table, List<LootResult<T>> results, Context? context = null, IRandom? random = null) {
            context ??= LootPipeline.EmptyContext;
            random ??= _defaultRandom;

            var workingSet = LootWorkingSetPool<T>.Get();
            try {
                workingSet.SourceTable = table;
                workingSet.Random = random;
                workingSet.CollectMetadata = _collectMetadata;

                BuildWeightedEntries(workingSet, table);

                foreach (var strategy in _strategies)
                    strategy.Process(workingSet, context);

                results.AddRange(workingSet.Results);

                NotifyObservers(table, results, context);
            } finally {
                LootWorkingSetPool<T>.Return(workingSet);
                LootPipeline.EmptyContext.Clear();
            }
        }

        /// <summary>
        /// Executes the pipeline and converts results using a factory.
        /// </summary>
        /// <typeparam name="TInstance">The instance type to create.</typeparam>
        /// <param name="table">The loot table to roll against.</param>
        /// <param name="factory">The factory to convert definitions to instances.</param>
        /// <param name="results">The list to append results to.</param>
        /// <param name="context">Optional context data.</param>
        /// <param name="random">Optional random override.</param>
        /// <returns>List of built loot results with instances.</returns>
        public void ExecuteAndBuild<TInstance>(ILootTable<T> table, ILootFactory<T, TInstance> factory, List<BuiltLootResult<T, TInstance>> results, Context? context = null,
            IRandom? random = null) {
            context ??= LootPipeline.EmptyContext;
            random ??= _defaultRandom;

            var rawResults = ListPool<LootResult<T>>.Get();
            try {
                Execute(table, rawResults, context, random);

                foreach (var result in rawResults) {
                    var instance = factory.Create(result.Item, context, random);
                    results.Add(new BuiltLootResult<T, TInstance> {
                        Definition = result.Item,
                        Instance = instance,
                        Quantity = result.Quantity,
                        Metadata = result.Metadata
                    });
                }
            } finally {
                ListPool<LootResult<T>>.Release(rawResults);
            }
        }

        private void NotifyObservers(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, Context context) {
            foreach (var observer in _observers)
                observer.OnRollComplete(table, results, context);
        }

        private static void BuildWeightedEntries(LootWorkingSet<T> workingSet, ILootTable<T> table) {
            var cumulative = 0f;
            var count = table.Count;

            for (var i = 0; i < count; i++) {
                var entry = table[i];
                cumulative += entry.Weight;
                workingSet.WeightedEntries.Add(new WeightedEntry<T> {
                    Entry = entry,
                    Index = i,
                    Weight = entry.Weight,
                    CumulativeWeight = cumulative
                });
            }

            workingSet.TotalWeight = cumulative;
        }
    }

    /// <summary>
    /// Simple object pool for LootWorkingSet to reduce allocations during high-frequency rolls.
    /// </summary>
    internal static class LootWorkingSetPool<T> {
        private static readonly Stack<LootWorkingSet<T>> Pool = new();

        public static LootWorkingSet<T> Get() {
            lock (Pool) {
                return Pool.Count > 0 ? Pool.Pop() : new LootWorkingSet<T>();
            }
        }

        public static void Return(LootWorkingSet<T> workingSet) {
            workingSet.Clear();
            lock (Pool) {
                Pool.Push(workingSet);
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