using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using UnityEngine;

public class FactoriesExamples {
    public enum Rarity {
        Common,
        Rare,
        Legendary
    }

    public static readonly Key<int> PlayerLevel = new("PlayerLevel");

    #region weaponFactory
    public class WeaponFactory : ILootFactory<WeaponDef, WeaponInstance> {
        public WeaponInstance Create(WeaponDef def, Context context, IRandom random) {
            var level = context.GetOrDefault(PlayerLevel, 1);

            return new WeaponInstance {
                Name = def.Name,
                Damage = def.DamageRange.Roll(random) + level,
                Rarity = def.Rarity,
                IsLegacy = def.Rarity == Rarity.Legendary && level >= 60
            };
        }
    }
    #endregion

    public void ExecuteAndBuildExample(LootPipeline<WeaponDef> pipeline, ILootTable<WeaponDef> weaponTable) {
        #region executeAndBuild
        var factory = new WeaponFactory();
        var context = new Context().Set(PlayerLevel, 42);

        var built = new List<BuiltLootResult<WeaponDef, WeaponInstance>>();
        pipeline.ExecuteAndBuild(weaponTable, factory, built, context);

        foreach (var r in built) {
            // inventory.Add(r.Instance, r.Quantity);

            // r.Definition is also available — useful for analytics / logging
            Debug.Log($"Rolled {r.Definition.Name} ? {r.Instance.Damage} dmg");
        }
        #endregion
    }

    #region scriptableObjectFactory
    // Definition — ScriptableObject in the project
    [CreateAssetMenu(menuName = "Items/Weapon")]
    public class WeaponDef : ScriptableObject {
        public string displayName;
        public IntRange damageRange;
        public IntRange speedRange;
        public Rarity rarity;
        public string Name => displayName;
        public IntRange DamageRange => damageRange;
        public Rarity Rarity => rarity;
    }

    // Instance — runtime data with rolled values
    public class WeaponInstance {
        public string Name { get; init; }
        public int Damage { get; init; }
        public int Speed { get; init; }
        public Rarity Rarity { get; init; }
        public bool IsLegacy { get; init; }
    }

    // Factory
    public class ScriptableWeaponFactory : ILootFactory<WeaponDef, WeaponInstance> {
        public WeaponInstance Create(WeaponDef def, Context ctx, IRandom random)
            => new() {
                Name = def.displayName,
                Damage = def.damageRange.Roll(random),
                Speed = def.speedRange.Roll(random),
                Rarity = def.rarity
            };
    }

    // Table — ScriptableObject in the editor
    [CreateAssetMenu(menuName = "Loot/Weapon Table")]
    public class WeaponLootTable : LootTableAsset<WeaponDef> { }
    #endregion

    public class UsageExample {
        WeaponLootTable _table;
        ScriptableWeaponFactory _factory;
        LootPipeline<WeaponDef> _pipeline;
        Context _context;

        void OnEnemyDeath() {
            #region scriptableUsage
            var built = new List<BuiltLootResult<WeaponDef, WeaponInstance>>();
            _pipeline.ExecuteAndBuild(_table.ToTable(), _factory, built, _context);
            foreach (var r in built) {
                // _inventory.Add(r.Instance, r.Quantity);
            }
            #endregion
        }
    }
}
