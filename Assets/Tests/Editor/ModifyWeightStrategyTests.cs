using System.Collections.Generic;
using System.Linq;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class ModifyWeightStrategyTests {
        private enum Item {
            Common,
            Rare
        }

        [Test]
        public void ModifyWeight_Multiplier_ScalesAllWeights() {
            var table = new LootTable<Item>()
                .Add(Item.Common, 4f)
                .Add(Item.Rare);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(ModifyWeightStrategy<Item>.Multiplier(2f))
                .AddStrategy(new WeightedRandomStrategy<Item>());

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void ModifyWeight_ZeroMultiplier_RemovesEntryFromSelection() {
            var table = new LootTable<Item>()
                .Add(Item.Common)
                .Add(Item.Rare);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new ModifyWeightStrategy<Item>((entry, _) => Equals(entry.Entry.Item, Item.Rare) ? 0f : entry.Weight))
                .AddStrategy(new WeightedRandomStrategy<Item>(20));

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            Assert.IsTrue(results.All(r => r.Item == Item.Common), "Rare should never appear after its weight is zeroed.");
        }

        [Test]
        public void ModifyWeight_MultiplierFromContext_UsesContextValue() {
            var luckKey = new Key<float>("Luck");
            var table = new LootTable<Item>().Add(Item.Common).Add(Item.Rare);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(ModifyWeightStrategy<Item>.MultiplierFromContext(luckKey))
                .AddStrategy(new WeightedRandomStrategy<Item>());

            var context = new Context().Set(luckKey, 0f);
            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results, context);
            Assert.AreEqual(0, results.Count);
        }
    }
}