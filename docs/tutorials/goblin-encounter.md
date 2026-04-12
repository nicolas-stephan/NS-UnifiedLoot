# Tutorial: The Goblin Encounter

This tutorial walks through the `GoblinEncounterExample` sample, demonstrating a real-world RPG loot system. We'll cover custom item definitions, complex pipelines, and advanced features like pity systems and luck-based scaling.

## 1. Defining the Loot

In a real project, loot is rarely just an `enum`. You often need metadata like item names, colors, icons, and base values.

### The Item Definition
We start by defining a `GoblinItemDef` which holds the static data for our items. This is what you would typically configure as a `ScriptableObject` or a serializable class in a list.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinItem.cs#goblinItemDef)]

### The Runtime Instance
When loot drops, we might want to create a unique instance of that item, potentially with randomized stats (e.g., varying gold value).

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinItem.cs#goblinItemInstance)]

---

## 2. Setting Up the Tables

UnifiedLoot supports designer-friendly `ScriptableObject` assets. To allow designers to edit tables in the Inspector, we create a simple class inheriting from `LootTableAsset<T>`.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinItemTable.cs)]

---

## 3. The Goblin Pipeline

For a regular Goblin, we want a simple but dynamic loot drop:
1.  **Luck Bonus**: Rare items should have a higher chance to drop if the player is lucky.
2.  **Multiple Rolls**: A goblin drops 3 items.
3.  **Bonus Roll**: Lucky players have a chance to get a 4th item.
4.  **Consolidation**: If multiple stacks of coins drop, they should be merged into one result.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinEncounterExample.cs#buildGoblinPipeline)]

---

## 4. The Goblin Captain (Advanced)

The Captain uses a more complex pipeline with **Advanced Scaling**.

### Luck-based Scaling
The Captain's pipeline uses luck to significantly increase the weight of legendary items.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinEncounterExample.cs#buildCaptainPipeline)]

---

## 5. Building the Loot (The Factory)

UnifiedLoot separates *selection* (what drops) from *instantiation* (creating the object). We use an `ILootFactory` to turn our `GoblinItemDef` into a `GoblinItemInstance`.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinEncounterExample.cs#factoryCreate)]

---

## 6. Execution

Finally, we trigger the roll. We use `ExecuteAndBuild` to perform the selection and the instantiation in one go.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Goblin/GoblinEncounterExample.cs#killGoblin)]

## Summary of Features Shown

- **[LootTableAsset<T>](xref:NS.UnifiedLoot.LootTableAsset`1)**: Creating designer-editable tables.
- **[ModifyWeightStrategy<T>](xref:NS.UnifiedLoot.ModifyWeightStrategy`1)**: Dynamically changing weights based on [Context](xref:NS.UnifiedLoot.Context).
- **[PityStrategy<T>](xref:NS.UnifiedLoot.PityStrategy`1)**: Implementing "bad luck protection".
- **[ILootFactory<TDef, TInstance>](xref:NS.UnifiedLoot.ILootFactory`2)**: Decoupling data from runtime objects.
- **[ExecuteAndBuild](xref:NS.UnifiedLoot.LootPipeline`1.ExecuteAndBuild*)**: End-to-end loot generation.
