using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NS.UnifiedLoot;

namespace NS.UnifiedLoot.Tests {
    public class CircularDependencyTests {
        private class ConcreteLootTableAsset : LootTableAsset<string> { }

        [Test]
        public void CircularDependency_SelfReference_Detected() {
            var table = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Circular dependency detected"));

            table.AddEntry(table);
            table.TriggerValidate();

            var stack = new List<LootTableAssetBase>();
            var hasCycle = table.HasCircularDependency(stack);
            Assert.IsTrue(hasCycle);
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(table, stack[0]);
            Assert.AreEqual(table, stack[1]);

            Assert.Throws<InvalidOperationException>(() => table.ToTable());
        }

        [Test]
        public void CircularDependency_DeepLoop_Detected() {
            var tableA = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var tableB = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var tableC = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Circular dependency detected"));

            tableA.AddEntry(tableB);
            tableB.AddEntry(tableC);
            tableC.AddEntry(tableA);
            tableA.TriggerValidate();

            var stack = new List<LootTableAssetBase>();
            Assert.IsTrue(tableA.HasCircularDependency(stack));
            Assert.AreEqual(4, stack.Count);
            Assert.AreEqual(tableA, stack[0]);
            Assert.AreEqual(tableB, stack[1]);
            Assert.AreEqual(tableC, stack[2]);
            Assert.AreEqual(tableA, stack[3]);

            Assert.Throws<InvalidOperationException>(() => tableA.ToTable());
        }

        [Test]
        public void NoCircularDependency_NotDetected() {
            var tableA = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();
            var tableB = ScriptableObject.CreateInstance<ConcreteLootTableAsset>();

            tableA.AddEntry(tableB);

            var stack = new List<LootTableAssetBase>();
            Assert.IsFalse(tableA.HasCircularDependency(stack));
            Assert.DoesNotThrow(() => tableA.ToTable());
        }
    }
}