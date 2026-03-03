using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class ScratchPadTests {
        private enum Item {
            Sword
        }

        private class WriterStrategy : ILootStrategy<Item> {
            public void Process(LootWorkingSet<Item> workingSet, LootContext context) => workingSet.ScratchPad["key"] = 42;
        }

        private class ReaderStrategy : ILootStrategy<Item> {
            public int ReadValue { get; private set; }

            public void Process(LootWorkingSet<Item> workingSet, LootContext context)
                => ReadValue = workingSet.ScratchPad.TryGetValue("key", out var v) ? (int)v : -1;
        }

        [Test]
        public void ScratchPad_StrategiesCanCommunicateViaIt() {
            var reader = new ReaderStrategy();
            var table = new LootTable<Item>().Add(Item.Sword);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(new WriterStrategy())
                .AddStrategy(reader);

            pipeline.Execute(table);

            Assert.AreEqual(42, reader.ReadValue);
        }

        [Test]
        public void ScratchPad_IsClearedBetweenExecutions() {
            var reader = new ReaderStrategy();
            var table = new LootTable<Item>().Add(Item.Sword);

            var writer = new WriterStrategy();
            var pipeline = new LootPipeline<Item>()
                .AddStrategy(writer)
                .AddStrategy(reader);

            pipeline.Execute(table);
            pipeline.RemoveStrategy(writer);
            pipeline.Execute(table);

            Assert.AreEqual(-1, reader.ReadValue, "ScratchPad should be cleared between executions.");
        }
    }
}