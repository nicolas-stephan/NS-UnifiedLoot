using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class SoftPityStrategyTests {
        private enum Item {
            Sword
        }

        private class NeverDropStrategy<T> : ILootStrategy<T> {
            public void Process(LootWorkingSet<T> workingSet, LootContext context) { }
        }

        [Test]
        public void SoftPity_HardPity_GuaranteesDropAfterNFailures() {
            var softPity = new SoftPityStrategy<Item>(softPityStart: 100, hardPityAt: 3);
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new NeverDropStrategy<Item>())
                .AddStrategy(softPity);

            var r1 = new List<LootResult<Item>>();
            pipeline.Execute(table, r1);
            var r2 = new List<LootResult<Item>>();
            pipeline.Execute(table, r2);
            Assert.AreEqual(0, r1.Count, "Run 1 should produce nothing.");
            Assert.AreEqual(0, r2.Count, "Run 2 should produce nothing.");

            var r3 = new List<LootResult<Item>>();
            pipeline.Execute(table, r3);
            Assert.AreEqual(1, r3.Count, "Run 3 should guarantee a drop at hard pity.");
        }

        [Test]
        public void SoftPity_ResetsAfterDrop() {
            var softPity = new SoftPityStrategy<Item>(softPityStart: 100, hardPityAt: 2);
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new NeverDropStrategy<Item>())
                .AddStrategy(softPity);

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            results.Clear();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0, softPity.GetFailureCount(table.Id));
        }

        [Test]
        public void SoftPity_IResettable_CanBeReset() {
            var softPity = new SoftPityStrategy<Item>(softPityStart: 100, hardPityAt: 10);
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new NeverDropStrategy<Item>())
                .AddStrategy(softPity);

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            pipeline.Execute(table, results);

            foreach (var r in pipeline.Strategies.OfType<IResettable>())
                r.ResetAll();

            Assert.AreEqual(0, softPity.GetFailureCount(table.Id));
        }
    }
}