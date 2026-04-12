using System.Collections.Generic;
using System.Linq;
using NS.UnifiedLoot;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class LootPipelineTests {
        private enum TestItem {
            Sword,
            Shield,
            Potion,
            Gold
        }

        [Test]
        public void WeightedRandom_ReturnsResultFromTable() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword)
                .Add(TestItem.Shield)
                .Add(TestItem.Potion);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>());

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, results.Count);
            Assert.Contains(results[0].Item, new[] { TestItem.Sword, TestItem.Shield, TestItem.Potion });
        }

        [Test]
        public void WeightedRandom_RollCount_ProducesCorrectNumberOfResults() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword)
                .Add(TestItem.Shield);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>(5));

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(5, results.Count);
        }

        [Test]
        public void DropChance_RollsEachEntryIndependently() {
            // With 100% drop chance, all items should drop
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword)
                .Add(TestItem.Shield)
                .Add(TestItem.Potion);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new DropChanceStrategy<TestItem>(weightAsPercent: false));

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(3, results.Count);
        }

        [Test]
        public void GuaranteedDrop_TriggersWhenNoResults() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new GuaranteedDropStrategy<TestItem>());

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void Filter_RemovesMatchingResults() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword)
                .Add(TestItem.Shield)
                .Add(TestItem.Potion);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new DropChanceStrategy<TestItem>())
                .AddStrategy(new FilterStrategy<TestItem>(r => r.Item != TestItem.Potion));

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(2, results.Count);
            Assert.IsFalse(results.Any(r => r.Item == TestItem.Potion));
        }

        [Test]
        public void Consolidate_CombinesDuplicateItems() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Gold, 1f, 10);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>(3))
                .AddStrategy(new ConsolidateResultsStrategy<TestItem>());

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(TestItem.Gold, results[0].Item);
            Assert.AreEqual(30, results[0].Quantity); // 3 rolls of 10
        }

        [Test]
        public void LimitResults_CapsResultCount() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword)
                .Add(TestItem.Shield)
                .Add(TestItem.Potion);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new DropChanceStrategy<TestItem>())
                .AddStrategy(new LimitResultsStrategy<TestItem>(2));

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void Quantity_RollsWithinRange() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Gold, 1f, 5, 10);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new WeightedRandomStrategy<TestItem>());

            var results = new List<LootResult<TestItem>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Quantity, 5);
            Assert.LessOrEqual(results[0].Quantity, 10);
        }
    }
}
