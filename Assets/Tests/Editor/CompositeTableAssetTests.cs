using System.Collections.Generic;
using System.Linq;
using System;
using NS.UnifiedLoot;
using NUnit.Framework;
using UnityEngine;

namespace NS.UnifiedLoot.Tests {
    public class LootTableAssetUnifiedTests {
        private enum TestItem {
            Sword,
            Potion,
            Shield
        }

        private class ConcreteLootTableAsset : LootTableAsset<TestItem> { }

        [Test]
        public void UnifiedTableAsset_CorrectlyFlattensMixedEntries() {
            var subTable = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            subTable.AddEntry(TestItem.Sword, 10f);

            var mainTable = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            mainTable.AddEntry(TestItem.Potion, 3f);
            mainTable.AddEntry(subTable, 7f);

            var runtimeTable = mainTable.ToTable();
            Assert.AreEqual(2, runtimeTable.Count);

            var list = new List<ILootEntry<TestItem>>();
            for (var i = 0; i < runtimeTable.Count; i++)
                list.Add(runtimeTable[i]);

            var potionEntry = list.FirstOrDefault(e => e.Item == TestItem.Potion);
            var swordEntry = list.FirstOrDefault(e => e.Item == TestItem.Sword);

            Assert.IsNotNull(potionEntry);
            Assert.IsNotNull(swordEntry);

            Assert.AreEqual(0.3f, potionEntry.Weight, 0.0001f);
            Assert.AreEqual(0.7f, swordEntry.Weight, 0.0001f);
        }

        [Test]
        public void UnifiedTableAsset_MultipliesQuantitiesForSubTables() {
            var subTable = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            subTable.AddEntry(TestItem.Sword, 1f, 5);

            var mainTable = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            mainTable.AddEntry(subTable, 1f, 3);

            var runtimeTable = mainTable.ToTable();
            Assert.AreEqual(1, runtimeTable.Count, "Entry count mismatch");
            Assert.AreEqual(15, runtimeTable[0].Quantity.Min, "Min quantity mismatch");
            Assert.AreEqual(15, runtimeTable[0].Quantity.Max, "Max quantity mismatch");
        }

        [Test]
        public void UnifiedTableAsset_SupportsDeepNesting() {
            var table1 = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var table2 = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var table3 = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();

            table3.AddEntry(TestItem.Shield);
            table2.AddEntry(table3);
            table1.AddEntry(table2);

            var runtimeTable = table1.ToTable();
            Assert.AreEqual(1, runtimeTable.Count);
            Assert.AreEqual(TestItem.Shield, runtimeTable[0].Item);
            Assert.AreEqual(1.0f, runtimeTable[0].Weight, 0.0001f);
        }

        [Test]
        public void UnifiedTableAsset_ComplexNestedStructure_FlattensCorrectly() {
            var mainTable = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var subTableA = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var subTableB = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            
            subTableA.AddEntry(TestItem.Sword, 2f);
            subTableA.AddEntry(TestItem.Potion, 3f);
            
            subTableB.AddEntry(TestItem.Shield, 1f, 2);
            subTableB.AddEntry(TestItem.Sword, 1f, 2);

            mainTable.AddEntry(TestItem.Potion, 5f);
            mainTable.AddEntry(subTableA, 10f);
            mainTable.AddEntry(subTableB, 5f, 3);

            var runtimeTable = mainTable.ToTable();

            Assert.AreEqual(5, runtimeTable.Count);

            // Weights Verification (Total = 20):
            // 1. Potion (Direct): W=5/20 = 0.25
            // 2. Sub-Table A (W=10/20 = 0.5):
            //    - Sword (A): W = 0.5 * (2/5) = 0.2
            //    - Potion (A): W = 0.5 * (3/5) = 0.3
            // 3. Sub-Table B (W=5/20 = 0.25):
            //    - Shield (B): W = 0.25 * (1/2) = 0.125
            //    - Sword (B): W = 0.25 * (1/2) = 0.125

            var results = new List<ILootEntry<TestItem>>();
            for (var i = 0; i < runtimeTable.Count; i++)
                results.Add(runtimeTable[i]);

            // Quantities:
            // Shield (B) should have Q = 2 (sub) * 3 (main) = 6
            // Sword (B) should have Q = 2 (sub) * 3 (main) = 6
            // Sword (A) should have Q = 1
            // Potion (A) should have Q = 1
            // Potion (Direct) should have Q = 1

            var shieldB = results.FirstOrDefault(e => e.Item == TestItem.Shield && Math.Abs(e.Weight - 0.125f) < 0.001f);
            var swordB = results.FirstOrDefault(e => e.Item == TestItem.Sword && Math.Abs(e.Weight - 0.125f) < 0.001f);
            var swordA = results.FirstOrDefault(e => e.Item == TestItem.Sword && Math.Abs(e.Weight - 0.2f) < 0.001f);
            var potionA = results.FirstOrDefault(e => e.Item == TestItem.Potion && Math.Abs(e.Weight - 0.3f) < 0.001f);
            var potionDirect = results.FirstOrDefault(e => e.Item == TestItem.Potion && Math.Abs(e.Weight - 0.25f) < 0.001f);

            Assert.IsNotNull(shieldB, "Shield B missing");
            Assert.IsNotNull(swordB, "Sword B missing");
            Assert.IsNotNull(swordA, "Sword A missing");
            Assert.IsNotNull(potionA, "Potion A missing");
            Assert.IsNotNull(potionDirect, "Potion Direct missing");

            Assert.AreEqual(6, shieldB.Quantity.Min);
            Assert.AreEqual(6, swordB.Quantity.Min);
            Assert.AreEqual(1, swordA.Quantity.Min);
            Assert.AreEqual(1, potionA.Quantity.Min);
            Assert.AreEqual(1, potionDirect.Quantity.Min);
        }
    }
}
