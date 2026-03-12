# Tutorial: Gacha & Loot Boxes

This tutorial explores the `GachaExample` sample, showing how to implement common gacha mechanics like hard/soft pity and multi-pulls using UnifiedLoot.

## 1. Pity Systems

Pity systems are essential for player satisfaction in gacha games. They ensure that players eventually receive high-value items after a certain number of attempts.

### Soft Pity
Soft pity increases the drop rate of rare items after a certain threshold of failures. In this example, we use `ModifyWeightStrategy` to boost the weight of 5-star items starting from the 70th pull.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#gachaPipeline)]

### Hard Pity
Hard pity guarantees a specific result after a fixed number of failures. We've implemented a custom `GachaHardPityStrategy` to handle this. It ensures a 4-star item every 10 pulls and a 5-star item every 90 pulls.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#customStrategy)]

## 2. Tracking Pity (The Observer)

To know when to reset the pity counters, we use an `ILootObserver`. It checks the results of each roll and resets the 4-star or 5-star counters if a matching item was dropped.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#gachaObserver)]

## 3. Multi-Pulls (10x)

A common feature is the "10-pull". To ensure that each pull correctly updates the pity counters for the *next* pull within the same set, we simply execute the pipeline 10 times in a loop.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#pullMethods)]

## Summary

The Gacha example demonstrates:
- **Dynamic Weighting**: Using `ModifyWeightStrategy` for soft pity ramps.
- **Custom Strategies**: Extending the system with `ILootGeneratorStrategy` for complex gacha rules.
- **State Management**: Using `ILootObserver` to synchronize persistent state (pity counters) with loot results.
