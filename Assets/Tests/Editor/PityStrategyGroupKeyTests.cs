using System.Collections.Generic;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class PityStrategyGroupKeyTests {
        private enum Item {
            Boss
        }

        private class NeverDropStrategy<T> : ILootStrategy<T> {
            public void Process(LootWorkingSet<T> workingSet, LootContext context) { }
        }

        [Test]
        public void PityGroupKey_SharedCounterAcrossTables() {
            const int groupKey = 999;
            var pity = new PityStrategy<Item>(maxFailures: 2, groupKey: groupKey);

            var tableA = new LootTable<Item>().Add(Item.Boss);
            var tableB = new LootTable<Item>().Add(Item.Boss);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new NeverDropStrategy<Item>())
                .AddStrategy(pity);

            var r1 = new List<LootResult<Item>>();
            pipeline.Execute(tableA, r1);
            Assert.AreEqual(0, r1.Count);

            var r2 = new List<LootResult<Item>>();
            pipeline.Execute(tableB, r2);
            Assert.AreEqual(1, r2.Count, "Shared pity counter should trigger across tables.");
        }

        [Test]
        public void PityStrategy_ImplementsIResettable() {
            var pity = new PityStrategy<Item>(3);
            Assert.IsInstanceOf<IResettable>(pity);
        }
    }
}