using System.Collections.Generic;
using NS.UnifiedLoot;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class BlackboardTests {
        private enum Item {
            Sword
        }

        private static readonly Key<int> TestKey = new("key");

        private class WriterStrategy : ILootStrategy<Item> {
            public void Process(LootWorkingSet<Item> workingSet, Context context) => workingSet.Blackboard.Set(TestKey, 42);
        }

        private class ReaderStrategy : ILootStrategy<Item> {
            public int ReadValue { get; private set; }

            public void Process(LootWorkingSet<Item> workingSet, Context context)
                => ReadValue = workingSet.Blackboard.GetOrDefault(TestKey, -1);
        }

        [Test]
        public void Blackboard_StrategiesCanCommunicateViaIt() {
            var reader = new ReaderStrategy();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WriterStrategy())
                .AddStrategy(reader);

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);

            Assert.AreEqual(42, reader.ReadValue);
        }

        [Test]
        public void Blackboard_IsClearedBetweenExecutions() {
            var reader = new ReaderStrategy();
            var table = new LootTable<Item>().Add(Item.Sword);

            var writer = new WriterStrategy();
            var pipeline = new LootPipeline<Item>()
                .AddStrategy(writer)
                .AddStrategy(reader);

            var results = new List<LootResult<Item>>();
            pipeline.Execute(table, results);
            pipeline.RemoveStrategy(writer);
            results.Clear();
            pipeline.Execute(table, results);

            Assert.AreEqual(-1, reader.ReadValue, "Blackboard should be cleared between executions.");
        }
    }
}
