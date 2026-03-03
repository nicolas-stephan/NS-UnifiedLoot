using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class LootContextTests {
        private static readonly ContextKey<int> PlayerLevel = new("PlayerLevel");
        private static readonly ContextKey<float> LuckStat = new("LuckStat");

        [Test]
        public void Context_StoresAndRetrievesValues() {
            var context = new LootContext();
            context.Set(PlayerLevel, 50);
            context.Set(LuckStat, 1.5f);

            Assert.AreEqual(50, context.Get(PlayerLevel));
            Assert.AreEqual(1.5f, context.Get(LuckStat));
        }

        [Test]
        public void Context_FluentApi_Works() {
            var context = new LootContext()
                .Set(PlayerLevel, 50)
                .Set(LuckStat, 1.5f);

            Assert.AreEqual(50, context.Get(PlayerLevel));
            Assert.AreEqual(1.5f, context.Get(LuckStat));
        }

        [Test]
        public void Context_GetOrDefault_ReturnsDefaultWhenMissing() {
            var context = new LootContext();

            Assert.AreEqual(0, context.GetOrDefault(PlayerLevel));
            Assert.AreEqual(1f, context.GetOrDefault(LuckStat, 1f));
        }

        [Test]
        public void Context_TryGet_ReturnsFalseWhenMissing() {
            var context = new LootContext();

            Assert.IsFalse(context.TryGet(PlayerLevel, out _));
        }

        [Test]
        public void Context_TryGet_ReturnsTrueWhenExists() {
            var context = new LootContext().Set(PlayerLevel, 50);

            Assert.IsTrue(context.TryGet(PlayerLevel, out var value));
            Assert.AreEqual(50, value);
        }

        [Test]
        public void Context_Contains_Works() {
            var context = new LootContext();
            Assert.IsFalse(context.Contains(PlayerLevel));

            context.Set(PlayerLevel, 50);
            Assert.IsTrue(context.Contains(PlayerLevel));
        }

        [Test]
        public void Context_Remove_Works() {
            var context = new LootContext().Set(PlayerLevel, 50);

            Assert.IsTrue(context.Contains(PlayerLevel));
            Assert.IsTrue(context.Remove(PlayerLevel));
            Assert.IsFalse(context.Contains(PlayerLevel));
            Assert.IsFalse(context.Remove(PlayerLevel));
        }

        [Test]
        public void Context_Clear_Works() {
            var context = new LootContext()
                .Set(PlayerLevel, 50)
                .Set(LuckStat, 1.5f);

            context.Clear();

            Assert.IsFalse(context.Contains(PlayerLevel));
            Assert.IsFalse(context.Contains(LuckStat));
        }

        [Test]
        public void Context_Set_OverwritesExistingValue() {
            var context = new LootContext().Set(PlayerLevel, 50);
            context.Set(PlayerLevel, 100);

            Assert.AreEqual(100, context.Get(PlayerLevel));
        }

        [Test]
        public void Context_Get_ThrowsWhenMissing() {
            var context = new LootContext();
            Assert.Throws<KeyNotFoundException>(() => context.Get(PlayerLevel));
        }

        [Test]
        public void Context_Set_ThrowsOnNullValue() {
            var context = new LootContext();
            var stringKey = new ContextKey<string>("Test");
            Assert.Throws<ArgumentNullException>(() => context.Set(stringKey, null!));
        }
    }
}