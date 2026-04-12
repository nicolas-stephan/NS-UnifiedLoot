using System.Collections.Generic;
using System.Linq;
using NS.UnifiedLoot;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class LootEntryEmptyTests {
        [Test]
        public void EmptyEntry_DoesNotProduceResult() {
            // Table: Sword (w=1) and Empty (w=1)
            // With many rolls, at most Sword should appear — empty entries never produce results
            var table = new LootTable<string>()
                .Add("Sword")
                .AddEmpty(1);

            var pipeline = new LootPipeline<string>()
                .AddStrategy(new WeightedRandomStrategy<string>(100));

            var results = new List<LootResult<string>>();
            pipeline.Execute(table, results);
            Assert.IsTrue(results.All(r => r.Item != null), "Empty entries should not produce null-item results.");
        }
    }
}
