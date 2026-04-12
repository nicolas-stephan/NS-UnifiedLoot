using System.Collections.Generic;
using NS.UnifiedLoot;

namespace NS.UnifiedLoot.Example.Doc {
    public class AdvancedExamples<T> {
        private LootPipeline<T> pipeline;
        private ILootTable<T> table;
        private Context context;
        private IRandom random;

        #region avoidingAllocations
        public class MyItem { }

        // Allocate once, reuse every time
        private List<LootResult<MyItem>> buffer = new();

        void DropLoot(ILootTable<MyItem> table, LootPipeline<MyItem> pipeline) {
            buffer.Clear();
            pipeline.Execute(table, buffer); // AddRange into buffer; no extra allocation
            ProcessResults(buffer);
        }

        void ProcessResults(List<LootResult<MyItem>> results) { }
        #endregion

        #region disablingMetadata
        void SetupPipeline(LootPipeline<T> pipeline) => pipeline.WithMetadata(false);
        #endregion

        #region deterministicRolls
        void Deterministic(ILootTable<T> table, LootPipeline<T> pipeline, Context context) {
            var random = new SystemRandom(seed: 42);
            var results = new List<LootResult<T>>();
            pipeline.Execute(table, results, context, random: random);

            // Or set the pipeline default
            pipeline.WithRandom(new SystemRandom(42));
        }
        #endregion

        #region blackboard
        public static class MyKeys {
            public static readonly Key<List<int>> SkippedEntries = new("SkippedEntries");
        }

        public class StrategyA : ILootStrategy<T> {
            public void Process(LootWorkingSet<T> ws, Context ctx) {
                var skipped = new List<int>();
                foreach (var entry in ws.WeightedEntries) {
                    if (ShouldSkip(entry))
                        skipped.Add(entry.Index);
                }

                ws.Blackboard.Set(MyKeys.SkippedEntries, skipped);
            }

            private bool ShouldSkip(WeightedEntry<T> entry) => false;
        }

        public class StrategyB : ILootStrategy<T> {
            public void Process(LootWorkingSet<T> ws, Context ctx) {
                if (ws.Blackboard.TryGet(MyKeys.SkippedEntries, out var skipped)) {
                    // handle skipped entries
                }
            }
        }
        #endregion

        #region introspection
        void Introspect(LootPipeline<T> pipeline, ILootStrategy<T> weightedRandom, ILootStrategy<T> myPreStrategy) {
            // View strategies
            IReadOnlyList<ILootStrategy<T>> strategies = pipeline.Strategies;

            // Insert at a specific position
            // Note: In actual code you might need to find the index first
            int idx = 0;
            pipeline.InsertStrategy(idx, myPreStrategy);

            // Remove a strategy
            pipeline.RemoveStrategy(myPreStrategy);
        }
        #endregion

        #region customRandom
        public class SeededXorShift : IRandom {
            uint _state;
            public SeededXorShift(uint seed) => _state = seed;

            uint Next() {
                _state ^= _state << 13;
                _state ^= _state >> 17;
                _state ^= _state << 5;
                return _state;
            }

            public float Value => (Next() & 0x00FFFFFF) / (float)0x01000000;

            public int Range(int min, int maxExclusive)
                => min + (int)(Value * (maxExclusive - min));

            public float Range(float min, float max)
                => min + Value * (max - min);
        }
        #endregion

        #region statefulStrategy
        public class MyStatefulStrategy : ILootStrategy<T>, IResettable {
            int _counter = 0;

            public void Process(LootWorkingSet<T> ws, Context ctx) {
                _counter++;
                // ...
            }

            public void ResetAll() => _counter = 0;
        }
        #endregion

        private LootPipeline<T> _pipeline;
        private ILootObserver<T> _myObserver;

        #region observerCleanup
        void OnDestroy() => _pipeline.RemoveObserver(_myObserver);
        #endregion
    }
}
