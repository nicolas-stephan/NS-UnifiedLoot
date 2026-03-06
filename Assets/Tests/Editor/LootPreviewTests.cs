using NUnit.Framework;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Preview;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

namespace NS.UnifiedLoot.Tests {
    public class LootPreviewTests {
        private enum TestItem {
            Sword,
            Potion
        }

        [Test]
        public void LootPreview_CorrectlyReflectsWeightAndQuantityModifications() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword, 5f, 1)
                .Add(TestItem.Potion, 2f, 7);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(ModifyWeightStrategy<TestItem>.Multiplier(2f))
                .AddStrategy(ModifyQuantityStrategy<TestItem>.Multiplier(2f));

            var preview = LootPreviewer.GetPreview(pipeline, table);

            Assert.AreEqual(14f, preview.TotalWeight, 0.001);
            Assert.AreEqual(2, preview.Entries.Count);

            var swordEntry = preview.Entries[0];
            Assert.AreEqual(TestItem.Sword, swordEntry.Item);
            Assert.AreEqual(5f, swordEntry.OriginalWeight, 0.001);
            Assert.AreEqual(10f, swordEntry.ModifiedWeight, 0.001);
            Assert.AreEqual(1, swordEntry.OriginalQuantity.Min);
            Assert.AreEqual(2, swordEntry.ModifiedQuantity.Min);

            var potionEntry = preview.Entries[1];
            Assert.AreEqual(TestItem.Potion, potionEntry.Item);
            Assert.AreEqual(2f, potionEntry.OriginalWeight, 0.001);
            Assert.AreEqual(4f, potionEntry.ModifiedWeight, 0.001);
            Assert.AreEqual(7, potionEntry.OriginalQuantity.Min);
            Assert.AreEqual(14, potionEntry.ModifiedQuantity.Min);
        }

        private class AddItemStrategy<T> : ILootTableModifierStrategy<T> {
            private readonly T _item;
            private readonly float _weight;

            public AddItemStrategy(T item, float weight) {
                _item = item;
                _weight = weight;
            }

            public void Process(LootWorkingSet<T> workingSet, Context context) {
                workingSet.WeightedEntries.Add(new WeightedEntry<T> {
                    Entry = new LootEntry<T>(_item, _weight),
                    Index = 1000 + workingSet.WeightedEntries.Count, // Some unique index
                    Weight = _weight,
                    CumulativeWeight = workingSet.TotalWeight + _weight
                });
                workingSet.TotalWeight += _weight;
            }
        }

        [Test]
        public void LootPreview_HandlesAddedEntriesFromStrategies() {
            var table = new LootTable<TestItem>()
                .Add(TestItem.Sword, 10f);

            var pipeline = new LootPipeline<TestItem>()
                .AddStrategy(new AddItemStrategy<TestItem>(TestItem.Potion, 10f));

            var preview = LootPreviewer.GetPreview(pipeline, table);

            Assert.AreEqual(2, preview.Entries.Count);
            Assert.AreEqual(TestItem.Sword, preview.Entries[0].Item);
            Assert.AreEqual(TestItem.Potion, preview.Entries[1].Item);
            Assert.AreEqual(0.5f, preview.Entries[0].Probability, 0.001f);
            Assert.AreEqual(0.5f, preview.Entries[1].Probability, 0.001f);
            Assert.AreEqual(0f, preview.Entries[1].OriginalWeight);
            Assert.AreEqual(10f, preview.Entries[1].ModifiedWeight);
        }
    }
}
