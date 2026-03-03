using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using NS.UnifiedLoot;

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
        private LootContext _context = null!;
        private List<LootResult<TestItem>> _reusableResults = null!;

        private static readonly ContextKey<float> LuckKey = new("Luck");

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

            _context = new LootContext().Set(LuckKey, 0.15f);
            _reusableResults = new List<LootResult<TestItem>>();
        }

        [Test, Performance]
        public void SimplePipeline_Execute_100000() {
            Measure.Method(() => _ = _simplePipeline.Execute(_table))
                .WarmupCount(5)
                .MeasurementCount(10)
                .IterationsPerMeasurement(100000)
                .Run();
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
        public void ComplexPipeline_Execute_100000() {
            Measure.Method(() => _ = _complexPipeline.Execute(_table, _context))
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

            Measure.Method(() => _ = pipeline.Execute(largeTable))
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10000)
                .Run();
        }
    }
}