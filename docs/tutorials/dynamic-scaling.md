# Tutorial: Dynamic Scaling

This tutorial explores the `ScalingExample` sample, showing how to adjust loot quality, quantity, and item stats dynamically based on the game's context (e.g., player level or difficulty).

## 1. Dynamic Weighting (Quality Scaling)

As a player levels up, you often want them to find better gear. Instead of creating a new loot table for every level, you can use `ModifyWeightStrategy` to adjust the probabilities of an existing table.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Scaling/ScalingExample.cs#scalingPipeline)]

In this example, the weights of "Poor" items decrease while "Rare" and "Epic" items increase as the `PlayerLevel` in the `Context` goes up.

## 2. Dynamic Quantity (Difficulty Scaling)

For harder encounters or higher game difficulties, you might want to drop *more* items. The `ModifyQuantityStrategy` allows you to scale the number of items produced by the generator.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Scaling/ScalingExample.cs#scalingPipeline)]

Here, the final quantity is multiplied by a `Difficulty` value provided in the context.

## 3. Stat Scaling (The Factory)

Finally, the items themselves should be more powerful at higher levels. Since UnifiedLoot separates selection from instantiation, the `ILootFactory` is the perfect place to inject this logic.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Scaling/ScalingExample.cs#scalingFactory)]

The factory receives the `Context`, allowing it to read the `PlayerLevel` and `Difficulty` to calculate the final `Power` of the `ScalingItemInstance`.

## 4. Execution

To trigger the roll, we pack the current game state into a `Context` and call `ExecuteAndBuild`.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Scaling/ScalingExample.cs#rollScalingLoot)]

## Summary

Dynamic scaling demonstrates:
- **Contextual Awareness**: Using the `Context` to pass game state into the loot system.
- **Probabilistic Adjustment**: Changing weights at runtime without modifying assets.
- **Factory-based Scaling**: Decoupling item data from its runtime power level.
