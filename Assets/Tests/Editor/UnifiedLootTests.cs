using System.Collections.Generic;
using NS.UnifiedLoot;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class ObserverTests {
        private enum Item {
            Sword
        }

        private class RecordingObserver : ILootObserver<Item> {
            public int CallCount { get; private set; }
            public IReadOnlyList<LootResult<Item>> LastResults { get; private set; } = new List<LootResult<Item>>();

            public void OnRollComplete(ILootTable<Item> table, IReadOnlyList<LootResult<Item>> results, Context context) {
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

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            Assert.AreEqual(1, observer.CallCount);

            results.Clear();
            pipeline.Execute(table, results);
            Assert.AreEqual(2, observer.CallCount);
        }

        [Test]
        public void Observer_ReceivesCorrectResults() {
            var observer = new RecordingObserver();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WeightedRandomStrategy<Item>())
                .AddObserver(observer);

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);

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

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            pipeline.RemoveObserver(observer);
            results.Clear();
            pipeline.Execute(table, results);

            Assert.AreEqual(1, observer.CallCount, "Observer should not be called after removal.");
        }
    }
}
