using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using UnityEngine;

public class TablesExamples {
    public class ItemDefinition { }
    public class Item { }
    
    private Item sword, shield, potion, gold;

    public void LootTableExample() {
        #region codeDefinedTable
        var table = new LootTable<Item>()
            .Add(sword, weight: 10f)
            .Add(shield, weight: 10f)
            .Add(potion, weight: 50f, minQuantity: 2, maxQuantity: 5)
            .Add(gold, weight: 30f, quantity: new IntRange(10, 50));
        #endregion

        #region addOverloads
        // Fixed quantity of 1
        table.Add(sword, weight: 10f);

        // Fixed quantity
        table.Add(sword, weight: 10f, quantity: 3);

        // Min/max quantity
        table.Add(sword, weight: 10f, minQuantity: 2, maxQuantity: 8);

        // Explicit IntRange
        table.Add(sword, weight: 10f, quantity: new IntRange(2, 8));
        #endregion

        #region nullItems
        var chestTable = new LootTable<Item>()
            .AddEmpty(weight: 50f) // 50% chance of nothing
            .Add(potion, weight: 30f)
            .Add(sword, weight: 20f);
        #endregion
    }

    #region scriptableObjectAsset
    [CreateAssetMenu(menuName = "Loot/Item Table")]
    public class ItemLootTable : LootTableAsset<ItemDefinition> { }
    #endregion

    public class TableUsageExample {
        [SerializeField] ItemLootTable _enemyDrops;
        LootPipeline<ItemDefinition> _pipeline = new();

        public void Roll() {
            #region tableAssetUsage
            // Drag the asset in the Inspector
            // [SerializeField] ItemLootTable _enemyDrops;

            // Should be cached
            var enemyRuntimeTable = _enemyDrops.ToTable();

            // At runtime
            var results = new List<LootResult<ItemDefinition>>();
            _pipeline.Execute(enemyRuntimeTable, results);
            #endregion
        }
    }

    public void CompositeExample(ILootTable<Item> commonTable, ILootTable<Item> rareTable) {
        #region compositeTable
        var composite = new CompositeTableBuilder<Item>()
            .Add(commonTable, selectionWeight: 0.7f)
            .Add(rareTable, selectionWeight: 0.3f)
            .Build();
        #endregion
    }

    public void IntRangeExample() {
        #region intRange
        var range = new IntRange(2, 8);
        int rolled = range.Roll(UnityRandom.Instance); // inclusive on both ends
        bool ok = range.Contains(5); // true

        // Implicit conversion from int (min == max)
        IntRange single = 3;
        #endregion
    }

    #region customTable
    public class ProcGenTable : ILootTable<Item> {
        public int Id { get; } = LootTableIdGenerator.GetNextId();
        public int Count => _entries.Count;
        public ILootEntry<Item> this[int index] => _entries[index];

        private readonly List<ILootEntry<Item>> _entries = new();

        // Procedurally fill _entries here...
    }
    #endregion
}
