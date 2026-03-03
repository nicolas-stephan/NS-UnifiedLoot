using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class ExpandResultsStrategyTests {
        private enum Item {
            Crate,
            Sword,
            Potion,
            Arrow
        }

        [Test]
        public void ExpandResults_ReplacesItemWithExpansion() {
            var table = new LootTable<Item>().Add(Item.Crate);
            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddStrategy(new ExpandResultsStrategy<Item>(item => item == Item.Crate ? new[] { Item.Sword, Item.Potion, Item.Arrow } : null));

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Any(r => r.Item == Item.Sword));
            Assert.IsTrue(results.Any(r => r.Item == Item.Potion));
            Assert.IsTrue(results.Any(r => r.Item == Item.Arrow));
        }

        [Test]
        public void ExpandResults_LeavesNonMatchingItemsUntouched() {
            var table = new LootTable<Item>()
                .Add(Item.Sword)
                .Add(Item.Potion, 0f);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddStrategy(new ExpandResultsStrategy<Item>(item => item == Item.Crate ? new[] { Item.Sword } : null));

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(Item.Sword, results[0].Item);
        }

        [Test]
        public void ExpandResults_QuantityResolver_IsApplied() {
            var table = new LootTable<Item>().Add(Item.Crate);
            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddStrategy(new ExpandResultsStrategy<Item>(item => item == Item.Crate ? new[] { Item.Arrow } : null, _ => 5));

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(Item.Arrow, results[0].Item);
            Assert.AreEqual(5, results[0].Quantity);
        }
    }
}