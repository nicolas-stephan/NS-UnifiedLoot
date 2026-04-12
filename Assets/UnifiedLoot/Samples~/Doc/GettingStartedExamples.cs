using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using NS.UnifiedLoot;
using UnityEngine;

public class GettingStartedSample {
    #region firstTable
    public enum Drop {
        Coin,
        Potion,
        Sword,
        Shield
    }

    private readonly LootTable<Drop> _table = new LootTable<Drop>()
        .Add(Drop.Coin, weight: 50f, minQuantity: 5, maxQuantity: 20)
        .Add(Drop.Potion, weight: 30f)
        .Add(Drop.Sword, weight: 10f)
        .Add(Drop.Shield, weight: 10f);
    #endregion

    #region firstPipeline
    private readonly LootPipeline<Drop> _pipeline = new LootPipeline<Drop>()
        .AddStrategy(new WeightedRandomStrategy<Drop>(rollCount: 3));
    #endregion

    private void Roll() {
        #region roll
        var results = new List<LootResult<Drop>>();
        _pipeline.Execute(_table, results);

        foreach (var result in results)
            Debug.Log($"Dropped: {result.Item} x{result.Quantity}");
        #endregion
    }

    private void GuaranteedPipeline() {
        #region guaranteedPipeline
        var pipeline = new LootPipeline<Drop>()
            .AddStrategy(new WeightedRandomStrategy<Drop>(3))
            .AddStrategy(new GuaranteedDropStrategy<Drop>());
        #endregion
    }

    private void ResuingPipeline() {
        LootPipeline<Drop> dropPipeline;
        LootTable<Drop> enemyTable = new();

        #region reusingPipeline
// In Awake / Start
        dropPipeline = new LootPipeline<Drop>()
            .WithMetadata(false) // skip metadata
            .AddStrategy(new WeightedRandomStrategy<Drop>(2))
            .AddStrategy(new ConsolidateResultsStrategy<Drop>());

// In game code (called many times)
        var results = new List<LootResult<Drop>>();
        dropPipeline.Execute(enemyTable, results);
        #endregion
    }
}
