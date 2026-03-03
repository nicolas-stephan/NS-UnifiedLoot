using System.Collections.Generic;

namespace NS.UnifiedLoot {
    internal static class LootPipeline {
        internal static readonly LootContext EmptyContext = new();
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
        public void Execute(ILootTable<T> table, List<LootResult<T>> results, LootContext? context = null, IRandom? random = null) {
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

        private void NotifyObservers(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, LootContext context) {
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
}