using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;
using UnityEngine;

public class ContextExamples {
    public class Item { }

    #region contextKeys
    public static class LootKeys {
        public static readonly Key<float> Luck = new("Luck");
        public static readonly Key<int> PlayerLevel = new("PlayerLevel");
        public static readonly Key<bool> IsBossRoom = new("IsBossRoom");
        public static readonly Key<int> MaxDrops = new("MaxDrops");
    }
    #endregion

    public void BuildingContext() {
        #region buildingContext
        var context = new Context()
            .Set(LootKeys.Luck, 1.5f)
            .Set(LootKeys.PlayerLevel, 42)
            .Set(LootKeys.IsBossRoom, true);
        #endregion

        #region readingContext
        // Throws if key is not present
        float luck = context.Get(LootKeys.Luck);

        // Returns default(T) if not present
        int level = context.GetOrDefault(LootKeys.PlayerLevel);

        // Returns supplied default if not present
        int levelWithDefault = context.GetOrDefault(LootKeys.PlayerLevel, defaultValue: 1);

        // Safe try-get
        if (context.TryGet(LootKeys.IsBossRoom, out bool isBoss)) {
            // ...
        }
        #endregion

        #region mutatingContext
        context.Set(LootKeys.Luck, 2.0f); // overwrite
        context.Remove(LootKeys.Luck); // delete key
        bool hasLuck = context.Contains(LootKeys.Luck); // check presence
        context.Clear(); // remove all keys
        #endregion
    }

    public void ExecuteWithContext(LootPipeline<Item> pipeline, ILootTable<Item> table, Context context) {
        #region passingContext
        var results = new List<LootResult<Item>>();
        pipeline.Execute(table, results, context);

        // Context is optional; pass null or omit it for a default empty context
        pipeline.Execute(table, results);
        #endregion
    }

    #region contextDrivenStrategy
    public class PlayerLevelScaler<T> : ILootStrategy<T> {
        public void Process(LootWorkingSet<T> workingSet, Context context) {
            if (!context.TryGet(LootKeys.PlayerLevel, out int level))
                return;

            // Scale all quantities by player level / 10
            float scale = level / 10f;
            for (var i = 0; i < workingSet.Results.Count; i++) {
                var r = workingSet.Results[i];
                workingSet.Results[i] = new LootResult<T>(r.Item, (int)(r.Quantity * scale), r.Metadata);
            }
        }
    }
    #endregion
}