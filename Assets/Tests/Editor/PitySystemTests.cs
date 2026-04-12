using NUnit.Framework;
using NS.UnifiedLoot;
using System.Collections.Generic;
using System.Linq;

namespace NS.UnifiedLoot.Tests {
    public class PitySystemTests {
        [Test]
        public void PityTracker_IncrementsFailures() {
            var tracker = new PityTracker();
            tracker.Record(1, PityResult.Failure);
            tracker.Record(1, PityResult.Failure);
            Assert.AreEqual(2, tracker.GetFailures(1));
        }

        [Test]
        public void PityTracker_ResetsOnSuccess() {
            var tracker = new PityTracker();
            tracker.Record(1, PityResult.Failure);
            tracker.Record(1, PityResult.Success); 
            Assert.AreEqual(0, tracker.GetFailures(1));
        }

        [Test]
        public void PityTracker_ResetAll_ClearsEverything() {
            var tracker = new PityTracker();
            tracker.Record(1, PityResult.Failure);
            tracker.Record(2, PityResult.Failure);
            tracker.ResetAll();
            Assert.AreEqual(0, tracker.GetFailures(1));
            Assert.AreEqual(0, tracker.GetFailures(2));
        }

        [Test]
        public void ItemPityStrategy_GuaranteesItemAfterThreshold() {
            var tracker = new PityTracker();
            var strategy = new ItemPityStrategy<string>(s => s.GetHashCode(), tracker);
            string rareItem = "RareSword";
            strategy.AddTrackedItem(rareItem, 3);

            var workingSet = new LootWorkingSet<string> {
                Random = new UnityRandom() // Should be mocked if possible, but let's assume it doesn't affect ForceDrop
            };

            // 1st failure
            strategy.Process(workingSet, new Context());
            Assert.AreEqual(1, tracker.GetFailures(rareItem.GetHashCode()));
            Assert.IsEmpty(workingSet.Results);

            // 2nd failure
            strategy.Process(workingSet, new Context());
            Assert.AreEqual(2, tracker.GetFailures(rareItem.GetHashCode()));
            Assert.IsEmpty(workingSet.Results);

            // 3rd failure -> Guaranteed
            strategy.Process(workingSet, new Context());
            Assert.AreEqual(0, tracker.GetFailures(rareItem.GetHashCode()));
            Assert.AreEqual(1, workingSet.Results.Count);
            Assert.AreEqual(rareItem, workingSet.Results[0].Item);
        }

        [Test]
        public void ItemPityStrategy_ResetsWhenItemDropsNormally() {
            var tracker = new PityTracker();
            var strategy = new ItemPityStrategy<string>(s => s.GetHashCode(), tracker);
            string rareItem = "RareSword";
            strategy.AddTrackedItem(rareItem, 3);

            var workingSet = new LootWorkingSet<string>();

            // 1 failure
            strategy.Process(workingSet, new Context());
            Assert.AreEqual(1, tracker.GetFailures(rareItem.GetHashCode()));

            // Normal drop
            workingSet.AddResult(rareItem, 1);
            strategy.Process(workingSet, new Context());
            Assert.AreEqual(0, tracker.GetFailures(rareItem.GetHashCode()));
            Assert.AreEqual(1, workingSet.Results.Count); // Still only 1 because it dropped normally
        }

        [Test]
        public void SharedTracker_WorksAcrossStrategies() {
            var tracker = new PityTracker();
            var strategy1 = new PityStrategy<string>(10, 1, tracker); // Group key 1
            var strategy2 = new PityStrategy<string>(10, 1, tracker); // Same group key 1

            var ws1 = new LootWorkingSet<string> { SourceTable = null }; // Uses groupKey=1

            strategy1.Process(ws1, new Context());
            Assert.AreEqual(1, tracker.GetFailures(1));

            strategy2.Process(ws1, new Context());
            Assert.AreEqual(2, tracker.GetFailures(1));
        }
    }
}
