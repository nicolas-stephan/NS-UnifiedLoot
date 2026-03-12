# Tutorial: Boss Loot Systems

This tutorial explores the `BossLootExample` sample, showing how to handle high-value encounters where loot is often dropped in multiple stages and certain item types must be guaranteed.

## 1. Multi-Stage Loot

In complex encounters like bosses, you often don't want a single roll. Instead, you might want to guarantee different categories of loot:
1.  **Currency**: A fixed amount or a guaranteed roll for gold/gems.
2.  **Materials**: Several rolls from a crafting table.
3.  **Equipment**: A high-stakes roll for a piece of gear.

By using multiple pipelines or executing the same pipeline against different tables, you can easily create these sequences.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Boss/BossLootExample.cs#multiStageRoll)]

## 2. Guaranteed Item Types

One common requirement is ensuring the player gets *at least one* item from a table, even if the table contains "Empty" results or low probabilities. The `GuaranteedDropStrategy` is perfect for this.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Boss/BossLootExample.cs#guaranteedDropStrategy)]

If the `WeightedRandomStrategy` results in 0 items (e.g., if all weights failed or an "Empty" entry was rolled), `GuaranteedDropStrategy` will perform an additional roll that ignores "Empty" entries to ensure a result.

## 3. Implementation Details

### Item Definitions
For this example, we use a `BossItemDef` that includes a rarity-based power calculation during the build process.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Boss/BossItem.cs#bossItemDef)]

### The Factory
The factory calculates the `Power` of the item instance based on its rarity and a small random variance.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Boss/BossLootExample.cs#bossFactory)]

## Summary

Boss loot systems demonstrate:
- **Sequential Execution**: Running multiple loot rolls into a single result set.
- **Fallbacks**: Using `GuaranteedDropStrategy` for player satisfaction.
- **Logic Injection**: Using the factory to scale item stats at runtime.
