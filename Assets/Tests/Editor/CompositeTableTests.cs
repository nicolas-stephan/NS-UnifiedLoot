using System.Linq;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class CompositeTableTests {
        private enum Item {
            Sword,
            Potion,
            Arrow
        }

        [Test]
        public void CompositeTable_EnumeratesAllEntries() {
            var tableA = new LootTable<Item>().Add(Item.Sword, 10f);
            var tableB = new LootTable<Item>().Add(Item.Potion, 10f);

            var composite = new CompositeTableBuilder<Item>().Add(tableA, 1f).Add(tableB, 1f).Build();

            Assert.AreEqual(2, composite.Count);
            Assert.IsTrue(composite.Any(e => Equals(e.Item, Item.Sword)), "Sword entry expected.");
            Assert.IsTrue(composite.Any(e => Equals(e.Item, Item.Potion)), "Potion entry expected.");
        }

        [Test]
        public void CompositeTable_ScalesWeightsProportionally() {
            // tableA (Sword w=10) → selection weight 0.3 → effective weight = 0.3
            // tableB (Potion w=10) → selection weight 0.7 → effective weight = 0.7
            var tableA = new LootTable<Item>().Add(Item.Sword, 10f);
            var tableB = new LootTable<Item>().Add(Item.Potion, 10f);

            var composite = new CompositeTableBuilder<Item>().Add(tableA, 0.3f).Add(tableB, 0.7f).Build();

            var entries = composite.ToList();
            Assert.AreEqual(2, entries.Count);

            var swordWeight = entries.First(e => Equals(e.Item, Item.Sword)).Weight;
            var potionWeight = entries.First(e => Equals(e.Item, Item.Potion)).Weight;

            Assert.AreEqual(0.3f, swordWeight, 0.0001f);
            Assert.AreEqual(0.7f, potionWeight, 0.0001f);
        }

        [Test]
        public void CompositeTable_MultipleEntriesPerSubTable_ScalesCorrectly() {
            // tableA: Sword(5) + Arrow(5) → each gets 0.3 * 0.5 = 0.15
            // tableB: Potion(10) → gets 0.7 * 1.0 = 0.70
            var tableA = new LootTable<Item>()
                .Add(Item.Sword, 5f)
                .Add(Item.Arrow, 5f);
            var tableB = new LootTable<Item>().Add(Item.Potion, 10f);

            var composite = new CompositeTableBuilder<Item>().Add(tableA, 0.3f).Add(tableB, 0.7f).Build();
            var entries = composite.ToList();

            var swordW = entries.First(e => Equals(e.Item, Item.Sword)).Weight;
            var arrowW = entries.First(e => Equals(e.Item, Item.Arrow)).Weight;
            var potionW = entries.First(e => Equals(e.Item, Item.Potion)).Weight;

            Assert.AreEqual(0.15f, swordW, 0.0001f);
            Assert.AreEqual(0.15f, arrowW, 0.0001f);
            Assert.AreEqual(0.70f, potionW, 0.0001f);
            Assert.AreEqual(1.0f, swordW + arrowW + potionW, 0.0001f);
        }

        [Test]
        public void CompositeTable_AddMany_WorksCorrectly() {
            var tableA = new LootTable<Item>().Add(Item.Sword, 10f);
            var tableB = new LootTable<Item>().Add(Item.Potion, 10f);

            var composite = new CompositeTableBuilder<Item>().AddMany(new[] { (tableA as ILootTable<Item>, 0.5f), (tableB as ILootTable<Item>, 0.5f) }).Build();

            Assert.AreEqual(2, composite.Count);
            Assert.AreEqual(0.5f, composite.First(e => Equals(e.Item, Item.Sword)).Weight, 0.0001f);
        }

    }
}