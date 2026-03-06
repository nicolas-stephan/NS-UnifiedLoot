using System.Collections.Generic;
using NS.UnifiedLoot;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using UnityEngine;

public class StrategiesExamples {
    private enum Rarity {
        Common,
        Rare,
        Legendary
    }

    private class Item {
        public Rarity Quality { get; set; }
        public int MinLevel { get; set; }
    }

    private class RareItem : Item { }

    private class ItemIdComparer : IEqualityComparer<Item> {
        public bool Equals(Item x, Item y) => true;
        public int GetHashCode(Item obj) => 0;
    }

    private void SelectionExamples() {
        #region weightedRandom
        // Roll 3 times
        new WeightedRandomStrategy<Item>(rollCount: 3);

        // Roll once, no duplicates
        new WeightedRandomStrategy<Item>(rollCount: 1, allowDuplicates: false);
        #endregion

        #region dropChance
        // Weights are 0–1 (0.05 = 5% chance)
        new DropChanceStrategy<Item>();

        // Weights are 0–100 (5.0 = 5% chance)
        new DropChanceStrategy<Item>(weightAsPercent: true);
        #endregion
    }

    private void GuaranteeExamples(LootPipeline<Item> pipeline) {
        #region guaranteedDrop
        pipeline.AddStrategy(new GuaranteedDropStrategy<Item>());
        #endregion
    }

    private void ModifyWeightExamples(LootPipeline<Item> pipeline, Key<float> luckKey, Key<int> contextKey) {
        #region modifyWeight
        // Multiply all weights by a fixed factor
        pipeline.AddStrategy(ModifyWeightStrategy<Item>.Multiplier(2f));

        // Read multiplier from context
        pipeline.AddStrategy(ModifyWeightStrategy<Item>.MultiplierFromContext(luckKey));

        // Scale based on context value range
        pipeline.AddStrategy(ModifyWeightStrategy<Item>.ScaleByContextRange(
            contextKey,
            range: new(1, 60),
            multiplier: 2.5f
        ));

        // Custom per-entry scaling
        pipeline.AddStrategy(new ModifyWeightStrategy<Item>((entry, context) => {
            float @base = entry.Weight;
            return entry.Entry.Item is RareItem ? @base * 2f : @base;
        }));
        #endregion
    }

    private void FilterExamples(LootPipeline<Item> pipeline, Key<int> playerLevelKey) {
        #region filter
        // Remove items below quality threshold
        pipeline.AddStrategy(new FilterStrategy<Item>(result => result.Item.Quality >= Rarity.Rare));
        #endregion

        #region filterByContext
        pipeline.AddStrategy(new FilterByContextStrategy<Item>((result, context) => {
            if (!context.TryGet(playerLevelKey, out int level))
                return true; // keep if no level context
            return result.Item.MinLevel <= level;
        }));
        #endregion
    }

    private void QuantityExamples(LootPipeline<Item> pipeline, Key<int> quantityBonusKey, Key<bool> isBossKey) {
        #region modifyQuantity
        // Multiply by a fixed factor (rounds down, min 1)
        pipeline.AddStrategy(ModifyQuantityStrategy<Item>.Multiplier(2));

        // Multiply by a context value
        pipeline.AddStrategy(ModifyQuantityStrategy<Item>.MultiplierFromContext(quantityBonusKey));

        // Custom logic
        pipeline.AddStrategy(new ModifyQuantityStrategy<Item>((qty, context) =>
            context.GetOrDefault(isBossKey) ? qty * 3 : qty));
        #endregion
    }

    private void ResultCountExamples(LootPipeline<Item> pipeline, Key<int> maxDropsKey) {
        #region limitResults
        pipeline.AddStrategy(new LimitResultsStrategy<Item>(maxResults: 5));
        #endregion

        #region limitResultsFromContext
        pipeline.AddStrategy(new LimitResultsFromContextStrategy<Item>(maxDropsKey));
        #endregion
    }

    private void PostProcessingExamples(LootPipeline<Item> pipeline) {
        #region consolidateResults
        // Use default equality (Equals/GetHashCode)
        pipeline.AddStrategy(new ConsolidateResultsStrategy<Item>());

        // Custom comparer (e.g. compare by item ID only, ignore quality)
        pipeline.AddStrategy(new ConsolidateResultsStrategy<Item>(new ItemIdComparer()));
        #endregion
    }
}