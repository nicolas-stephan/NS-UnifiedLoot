using System.Collections.Generic;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class ObserverTests {
        private enum Item {
            Sword
        }

        private class RecordingObserver : ILootObserver<Item> {
            public int CallCount { get; private set; }
            public IReadOnlyList<LootResult<Item>> LastResults { get; private set; } = new List<LootResult<Item>>();

            public void OnRollComplete(ILootTable<Item> table, IReadOnlyList<LootResult<Item>> results, LootContext context) {
                CallCount++;
                LastResults = results;
            }
        }

        [Test]
        public void Observer_IsCalledAfterEveryRoll() {
            var observer = new RecordingObserver();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddObserver(observer);

            pipeline.Execute(table);
            Assert.AreEqual(1, observer.CallCount);

            pipeline.Execute(table);
            Assert.AreEqual(2, observer.CallCount);
        }

        [Test]
        public void Observer_ReceivesCorrectResults() {
            var observer = new RecordingObserver();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddObserver(observer);

            pipeline.Execute(table);

            Assert.IsNotNull(observer.LastResults);
            Assert.AreEqual(1, observer.LastResults.Count);
            Assert.AreEqual(Item.Sword, observer.LastResults[0].Item);
        }

        [Test]
        public void RemoveObserver_StopsNotifications() {
            var observer = new RecordingObserver();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddObserver(observer);

            pipeline.Execute(table);
            pipeline.RemoveObserver(observer);
            pipeline.Execute(table);

            Assert.AreEqual(1, observer.CallCount, "Observer should not be called after removal.");
        }
    }
}