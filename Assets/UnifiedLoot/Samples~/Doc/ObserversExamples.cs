using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using UnityEngine;

public class ObserversExamples {
    public class ItemDef {
        public Rarity Rarity { get; set; }
    }

    public enum Rarity {
        Common,
        Rare,
        Legendary
    }

    public void RegisterExample(LootPipeline<ItemDef> pipeline) {
        #region registerObserver
        var analytics = new LootAnalyticsObserver<ItemDef>();
        var logger = new LootDebugLogger<ItemDef>();

        pipeline.AddObserver(analytics);
        pipeline.AddObserver(logger);

        // Remove when no longer needed
        pipeline.RemoveObserver(logger);
        #endregion
    }

    #region analyticsObserver
    public class LootAnalyticsObserver<T> : ILootObserver<T> {
        public void OnRollComplete(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, Context context) {
            foreach (var result in results) {
                // Analytics.LogEvent("loot_dropped", new Dictionary<string, object> {
                //     ["table_id"] = table.Id,
                //     ["item"]     = result.Item?.ToString() ?? "none",
                //     ["quantity"] = result.Quantity,
                // });
            }
        }
    }
    #endregion

    #region debugLoggerObserver
    public class LootDebugLogger<T> : ILootObserver<T> {
        private readonly string _tag;

        public LootDebugLogger(string tag = "Loot") => _tag = tag;

        public void OnRollComplete(ILootTable<T> table, IReadOnlyList<LootResult<T>> results, Context context) {
            if (results.Count == 0) {
                Debug.Log($"[{_tag}] Table {table.Id}: no drops");
                return;
            }

            foreach (var r in results)
                Debug.Log($"[{_tag}] Table {table.Id}: {r.Item} x{r.Quantity}");
        }
    }
    #endregion

    #region achievementObserver
    public class AchievementObserver : ILootObserver<ItemDef> {
        // readonly AchievementSystem _achievements;

        // public AchievementObserver(AchievementSystem achievements)
        //     => _achievements = achievements;

        public void OnRollComplete(ILootTable<ItemDef> table, IReadOnlyList<LootResult<ItemDef>> results, Context context) {
            foreach (var r in results)
                if (r.Item.Rarity == Rarity.Legendary) {
                    // _achievements.Unlock(AchievementId.FirstLegendary);
                }
        }
    }
    #endregion
}
