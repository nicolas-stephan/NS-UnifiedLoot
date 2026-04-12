using System.Collections.Generic;
using NS.UnifiedLoot;
using NUnit.Framework;

namespace NS.UnifiedLoot.Tests {
    public class LootFactoryTests {
        private struct WeaponDefinition {
            public string Name;
            public IntRange DamageRange;
            public float CritChance;
        }

        private class WeaponInstance {
            public string Name = string.Empty;
            public int Damage;
            public float CritChance;
        }

        private class WeaponFactory : ILootFactory<WeaponDefinition, WeaponInstance> {
            public WeaponInstance Create(WeaponDefinition definition, Context context, IRandom random) {
                return new WeaponInstance {
                    Name = definition.Name,
                    Damage = definition.DamageRange.Roll(random),
                    CritChance = definition.CritChance
                };
            }
        }

        private class LevelScalingWeaponFactory : ILootFactory<WeaponDefinition, WeaponInstance> {
            private readonly Key<int> _levelKey;

            public LevelScalingWeaponFactory(Key<int> levelKey) { _levelKey = levelKey; }

            public WeaponInstance Create(WeaponDefinition definition, Context context, IRandom random) {
                var level = context.GetOrDefault(_levelKey, 1);
                return new WeaponInstance {
                    Name = definition.Name,
                    Damage = definition.DamageRange.Min + level, // Simple scaling
                    CritChance = definition.CritChance
                };
            }
        }

        [Test]
        public void ExecuteAndBuild_CreatesInstances() {
            var table = new LootTable<WeaponDefinition>()
                .Add(new WeaponDefinition { Name = "Sword", DamageRange = new IntRange(10, 20), CritChance = 0.1f })
                .Add(new WeaponDefinition { Name = "Axe", DamageRange = new IntRange(15, 30), CritChance = 0.05f });

            var pipeline = new LootPipeline<WeaponDefinition>()
                .AddStrategy(new WeightedRandomStrategy<WeaponDefinition>());

            var factory = new WeaponFactory();
            var results = new List<BuiltLootResult<WeaponDefinition, WeaponInstance>>();
            pipeline.ExecuteAndBuild(table, factory, results);

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0].Instance);
            Assert.IsNotNull(results[0].Definition.Name);
            Assert.AreEqual(results[0].Definition.Name, results[0].Instance.Name);
            Assert.GreaterOrEqual(results[0].Instance.Damage, results[0].Definition.DamageRange.Min);
            Assert.LessOrEqual(results[0].Instance.Damage, results[0].Definition.DamageRange.Max);
        }

        [Test]
        public void ExecuteAndBuild_PreservesQuantityAndMetadata() {
            var table = new LootTable<WeaponDefinition>()
                .Add(new WeaponDefinition { Name = "Sword", DamageRange = new IntRange(10, 20), CritChance = 0.1f }, 1f, 3, 5);

            var pipeline = new LootPipeline<WeaponDefinition>()
                .AddStrategy(new WeightedRandomStrategy<WeaponDefinition>());

            var factory = new WeaponFactory();
            var results = new List<BuiltLootResult<WeaponDefinition, WeaponInstance>>();
            pipeline.ExecuteAndBuild(table, factory, results);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Quantity, 3);
            Assert.LessOrEqual(results[0].Quantity, 5);
            Assert.AreEqual(table.Id, results[0].Metadata.SourceTableId);
        }

        [Test]
        public void ExecuteAndBuild_FactoryReceivesContext() {
            var levelKey = new Key<int>("PlayerLevel");

            var table = new LootTable<WeaponDefinition>()
                .Add(new WeaponDefinition { Name = "Sword", DamageRange = new IntRange(10, 20), CritChance = 0.1f });

            var pipeline = new LootPipeline<WeaponDefinition>()
                .AddStrategy(new WeightedRandomStrategy<WeaponDefinition>());

            var factory = new LevelScalingWeaponFactory(levelKey);
            var context = new Context().Set(levelKey, 10);

            var results = new List<BuiltLootResult<WeaponDefinition, WeaponInstance>>();
            pipeline.ExecuteAndBuild(table, factory, results, context);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Instance.Damage, 10 + 10);
        }
    }
}
