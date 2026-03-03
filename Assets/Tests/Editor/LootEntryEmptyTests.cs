using System.Linq;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class LootEntryEmptyTests {
        [Test]
        public void EmptyEntry_DoesNotProduceResult() {
            // Table: Sword (w=1) and Empty (w=1)
            // With many rolls, at most Sword should appear — empty entries never produce results
            var table = new LootTable<string>()
                .Add("Sword")
                .Add(LootEntry.Empty<string>());

            var pipeline = new LootPipeline<string>()
                .AddStrategy(new WeightedRandomStrategy<string>(100));

            var results = pipeline.Execute(table);
            Assert.IsTrue(results.All(r => r.Item != null), "Empty entries should not produce null-item results.");
        }

        [Test]
        public void EmptyEntry_Item_IsDefault() {
            var entry = LootEntry.Empty<string>(2f);
            Assert.IsNull(entry.Item);
            Assert.AreEqual(2f, entry.Weight);
        }
    }
}