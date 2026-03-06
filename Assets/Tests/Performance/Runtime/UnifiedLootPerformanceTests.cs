using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace NS.UnifiedLoot.Tests.Performance {
    public class LootPipelinePerformanceTests {
        private enum TestItem {
            Item1,
            Item2,
            Item3,
            Item4,
            Item5,
            Item6,
            Item7,
            Item8,
            Item9,
            Item10
        }

        private LootTable<TestItem> _table = null!;
        private LootPipeline<TestItem> _simplePipeline = null!;
        private LootPipeline<TestItem> _complexPipeline = null!;
        private Context _context = null!;
        private List<LootResult<TestItem>> _reusableResults = null!;

        private static readonly Key<float> LuckKey = new("Luck");

        [SetUp]
        public void Setup() {
            _table = new LootTable<TestItem>()
                .Add(TestItem.Item1, 10f)
                .Add(TestItem.Item2, 20f)
                .Add(TestItem.Item3, 30f)
                .Add(TestItem.Item4, 15f)
                .Add(TestItem.Item5, 25f)
                .Add(TestItem.Item6, 5f, 1, 10)
                .Add(TestItem.Item7, 8f, 1, 5)
                .Add(TestItem.Item8, 12f)
                .Add(TestItem.Item9, 18f)
                .Add(TestItem.Item10, 7f, 2, 8);

            _simplePipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>(3));

            _complexPipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>(5))
                .AddStrategy(new BonusRollStrategy<TestItem>(LuckKey, 0.1f))
                .AddStrategy(new FilterStrategy<TestItem>(r => r.Item != TestItem.Item1))
                .AddStrategy(new ConsolidateResultsStrategy<TestItem>())
                .AddStrategy(new LimitResultsStrategy<TestItem>(10));

            _context = new Context().Set(LuckKey, 0.15f);
            _reusableResults = new List<LootResult<TestItem>>();
        }
        
        [Test, Performance]
        public void SimplePipeline_ExecuteNoAlloc_100000() {
            Measure.Method(() => {
                    _reusableResults.Clear();
                    _simplePipeline.Execute(_table, _reusableResults);
                })
                .WarmupCount(5)
                .MeasurementCount(10)
                .IterationsPerMeasurement(100000)
                .Run();
        }

        [Test, Performance]
        public void ComplexPipeline_ExecuteNoAlloc_100000() {
            Measure.Method(() => {
                    _reusableResults.Clear();
                    _complexPipeline.Execute(_table, _reusableResults, _context);
                })
                .WarmupCount(5)
                .MeasurementCount(10)
                .IterationsPerMeasurement(100000)
                .Run();
        }

        [Test, Performance]
        public void LargeTable_WeightedRandom_10000() {
            var largeTable = new LootTable<int>();
            for (var i = 0; i < 1000; i++)
                largeTable.Add(i, UnityEngine.Random.Range(1f, 100f));

            var pipeline = new LootPipeline<int>()
                .AddStrategy(new WeightedRandomStrategy<int>(10));

            var results = new List<LootResult<int>>();
            Measure.Method(() => {
                    results.Clear();
                    pipeline.Execute(largeTable, results);
                })
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10000)
                .Run();
        }

        [Test, Performance]
        public void CompositeTable_Execute_10000() {
            var builder = new CompositeTableBuilder<int>();
            for (var i = 0; i < 10; i++) {
                var t = new LootTable<int>();
                for (var j = 0; j < 100; j++)
                    t.Add(i * 100 + j, UnityEngine.Random.Range(1f, 100f));
                builder.Add(t, 1f);
            }
            var composite = builder.Build();

            var pipeline = new LootPipeline<int>()
                .AddStrategy(new WeightedRandomStrategy<int>(10));

            var results = new List<LootResult<int>>();
            Measure.Method(() => {
                    results.Clear();
                    pipeline.Execute(composite, results);
                })
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10000)
                .Run();
        }
    }
}