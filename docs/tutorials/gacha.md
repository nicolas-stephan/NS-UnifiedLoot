# Tutorial: Gacha & Loot Boxes

This tutorial explores the `GachaExample` sample, showing how to implement common gacha mechanics like hard/soft pity and multi-pulls using UnifiedLoot. We'll also see the correct way to reuse a `Context` across multiple rolls.

## 1. Context Reuse

When performing multiple rolls (like a "10-pull"), it is more efficient to reuse a single `Context` instance. This avoids unnecessary allocations and allows strategies and observers to persist data more easily throughout the sequence.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#pullMethods)]

## 2. Cohesive Pity System

Instead of splitting pity logic across multiple unrelated classes, we can implement a cohesive `GachaPitySystem` that handles all stages of the gacha lifecycle. By implementing both `ILootStrategy` and `ILootObserver`, it can manage state, modify weights, and enforce guarantees in one place.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#pitySystem)]

### Soft Pity
Soft pity increases the drop rate of rare items after a certain threshold of failures. We use `ModifyWeightStrategy` and pass it a method from our pity system that calculates the boost based on the internal `IPityTracker`.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Gacha/GachaExample.cs#gachaPipeline)]

### Hard Pity
Hard pity guarantees a specific result after a fixed number of failures. The `GachaPitySystem` checks the results of the roll and, if the threshold is reached without a high-rarity drop, it clears the current results and forces a filtered roll from the table.

## Summary

The Gacha example demonstrates:
- **Context Reuse**: Reusing a single `Context` instance across multiple rolls for efficiency and data persistence.
- **Dynamic Weighting**: Using `ModifyWeightStrategy` for soft pity ramps.
- **Cohesive Pity System**: Implementing multiple interfaces in a single class to manage state, modify weights, and enforce hard pity.
- **State Management**: Using `IPityTracker` and `ILootObserver` to synchronize persistent state with loot results.
