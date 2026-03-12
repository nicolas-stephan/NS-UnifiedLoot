# Preview System

The Preview System allows you to perform "dry runs" of your loot tables through a pipeline. This is essential for debugging complex strategy chains, showing potential outcomes to players, or validating drop rates in the Unity Editor.

> [!IMPORTANT]
> Unlike a standard [Execute](xref:NS.UnifiedLoot.LootPipeline`1.Execute*), a preview does not perform any random rolls. Instead, it calculates the modified weights and probabilities for every entry in the table after all strategies have been applied.

## Using the Editor Preview

The easiest way to see the preview system in action is through the **Pipeline Preview Simulation** found on any [LootTableAsset](tables.md#loottableasset--scriptableobject).

1. Select a [LootTableAsset](tables.md#loottableasset--scriptableobject) in your Project window.
2. In the Inspector, expand the **Pipeline Preview Simulation** foldout.
3. This tool simulates a [ModifyWeightStrategy](strategies.md#modifyweightstrategy) applied to the table.
4. You can see how the "Original Weight" compares to the "Modified Weight", and what the resulting drop probability (%) will be.

> [!NOTE]
> The Editor Preview is a built-in tool to help you visualize how pipelines modify tables. It demonstrates the math behind the strategies before any items are actually rolled.

## Previewing in Code

You can generate a preview programmatically using the [LootPreviewer](xref:NS.UnifiedLoot.LootPreviewer) utility class.

```csharp
// 1. Setup your pipeline and table
var pipeline = new LootPipeline<MyItem>();
pipeline.AddStrategy(ModifyWeightStrategy<MyItem>.Multiplier(2.0f));

var table = new LootTable<MyItem>();
// ... populate table ...

// 2. Request a preview
LootTablePreview<MyItem> preview = LootPreviewer.GetPreview(pipeline, table);

// 3. Inspect the results
Debug.Log($"Total Weight: {preview.TotalWeight}");

foreach (var entry in preview.Entries)
{
    Debug.Log($"Item: {entry.Item}, Prob: {entry.Probability * 100}%");
    Debug.Log($"Weight: {entry.OriginalWeight} -> {entry.ModifiedWeight}");
}
```

## Preview Data Structure

The `GetPreview` method returns a [LootTablePreview<T>](xref:NS.UnifiedLoot.LootTablePreview`1) object containing:

| Property | Type | Description |
| :--- | :--- | :--- |
| `Entries` | `List<LootPreviewEntry<T>>` | Detailed information for each potential outcome. |
| `TotalWeight` | `float` | The sum of all modified weights in the table. |

Each `LootPreviewEntry<T>` provides:

*   **Item**: The reference to the item.
*   **OriginalWeight / ModifiedWeight**: The weight before and after pipeline processing.
*   **OriginalQuantity / ModifiedQuantity**: The quantity range before and after processing.
*   **Probability**: The 0-1 chance of this item being picked in a single roll.

## How Strategies Support Previews

The `LootPreviewer` automatically handles strategies that implement [ILootTableModifierStrategy<T>](xref:NS.UnifiedLoot.ILootTableModifierStrategy`1) (like weight modifiers) because they directly affect the `LootWorkingSet`.

However, some strategies might want to show changes that don't happen in the "table modification" phase. Strategies can implement the `OnPreview` method from [ILootStrategy<T>](xref:NS.UnifiedLoot.ILootStrategy`1) to provide custom preview data:

```csharp
public class MyCustomStrategy<T> : ILootStrategy<T>, ILootResultModifierStrategy<T>
{
    public void OnPreview(LootTablePreview<T> preview, Context context)
    {
        // For example, a strategy that doubles all quantities:
        foreach (var entry in preview.Entries)
        {
            entry.ModifiedQuantity = new IntRange(
                entry.OriginalQuantity.Min * 2,
                entry.OriginalQuantity.Max * 2
            );
        }
    }

    // ... other interface implementations ...
}
```

> [!TIP]
> Strategies that purely generate results (like `WeightedRandomStrategy`) are typically ignored during previews, as previews focus on the *state* of the table rather than specific *outcomes*.
