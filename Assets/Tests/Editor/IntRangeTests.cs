using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class IntRangeTests {
        [Test]
        public void IntRange_SingleValue_ReturnsConstant() {
            var range = new IntRange(5);
            var random = new SystemRandom(42);

            Assert.AreEqual(5, range.Roll(random));
        }

        [Test]
        public void IntRange_Range_ReturnsWithinBounds() {
            var range = new IntRange(1, 100);
            var random = new SystemRandom(42);

            for (var i = 0; i < 100; i++) {
                var value = range.Roll(random);
                Assert.GreaterOrEqual(value, 1);
                Assert.LessOrEqual(value, 100);
            }
        }
    }
}