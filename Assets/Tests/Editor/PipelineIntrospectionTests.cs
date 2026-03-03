using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class PipelineIntrospectionTests {
        private enum Item { }

        [Test]
        public void Strategies_ReturnsAllAddedStrategies() {
            var s1 = new WeightedRandomStrategy<Item>();
            var s2 = new ConsolidateResultsStrategy<Item>();

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(s1)
                .AddStrategy(s2);

            Assert.AreEqual(2, pipeline.Strategies.Count);
            Assert.AreSame(s1, pipeline.Strategies[0]);
            Assert.AreSame(s2, pipeline.Strategies[1]);
        }

        [Test]
        public void InsertStrategy_InsertsAtCorrectIndex() {
            var s1 = new WeightedRandomStrategy<Item>();
            var s2 = new ConsolidateResultsStrategy<Item>();
            var s3 = new LimitResultsStrategy<Item>(1);

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(s1)
                .AddStrategy(s3)
                .InsertStrategy(1, s2);

            Assert.AreEqual(3, pipeline.Strategies.Count);
            Assert.AreSame(s2, pipeline.Strategies[1]);
            Assert.AreSame(s3, pipeline.Strategies[2]);
        }

        [Test]
        public void RemoveStrategy_RemovesCorrectStrategy() {
            var s1 = new WeightedRandomStrategy<Item>();
            var s2 = new ConsolidateResultsStrategy<Item>();

            var pipeline = new LootPipeline<Item>()
                .AddStrategy(s1)
                .AddStrategy(s2)
                .RemoveStrategy(s1);

            Assert.AreEqual(1, pipeline.Strategies.Count);
            Assert.AreSame(s2, pipeline.Strategies[0]);
        }
    }
}