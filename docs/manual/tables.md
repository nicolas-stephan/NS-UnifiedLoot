# Tables

A loot table is anything that implements [ILootTable<T>](xref:NS.UnifiedLoot.ILootTable`1). It is a read-only collection of
[ILootEntry<T>](xref:NS.UnifiedLoot.ILootEntry`1) values. The pipeline reads from it but never mutates it.

## [ILootTable<T>](xref:NS.UnifiedLoot.ILootTable`1)

```csharp
public interface ILootTable<out T> : IEnumerable<ILootEntry<T>> {
    int Id { get; } // auto-generated int, used by pity tracking
    int Count { get; }
    ILootEntry<T> this[int index] { get; }
}
```

## [ILootEntry<T>](xref:NS.UnifiedLoot.ILootEntry`1)

```csharp
public interface ILootEntry<out T> {
    T? Item { get; } // nullable — null means "no drop"
    float Weight { get; }
    IntRange Quantity { get; }
}
```

## [LootTable<T>](xref:NS.UnifiedLoot.LootTable`1) - Code-Defined

The simplest table type. Fluent builder API, good for tables you define in code.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#codeDefinedTable)]

### Add overloads

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#addOverloads)]

### Null items

Use [AddEmpty](xref:NS.UnifiedLoot.LootTable`1.AddEmpty*) to add a "no drop" slot. When selected, the entry
is silently skipped and produces no [LootResult<T>](xref:NS.UnifiedLoot.LootResult`1). This is useful for modelling a chance of
getting nothing:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#nullItems)]

## [LootTableAsset<T>](xref:NS.UnifiedLoot.LootTableAsset`1) — ScriptableObject

For tables that designers configure in the Unity Editor:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#scriptableObjectAsset)]

Create the asset in the Project window. The custom inspector shows:
- A probability summary for every entry
- Live validation warnings (null items, zero weight, empty table, single-entry table)

> [!NOTE]
> `LootTableAsset<T>` does **not** require `T` to be a `ScriptableObject` — any serialisable
> type works.

### Passing to a pipeline

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#tableAssetUsage)]

### [LootTableAssetBase](xref:NS.UnifiedLoot.LootTableAssetBase)

All [LootTableAsset<T>](xref:NS.UnifiedLoot.LootTableAsset`1) inherit from the non-generic [LootTableAssetBase](xref:NS.UnifiedLoot.LootTableAssetBase). This lets you write
editor code (custom drawers, validators) that targets all table assets without knowing `T`.

## [CompositeTableBuilder<T>](xref:NS.UnifiedLoot.CompositeTableBuilder`1)

The [CompositeTableBuilder<T>](xref:NS.UnifiedLoot.CompositeTableBuilder`1) allows you to merge multiple sub-tables into a single, flattened table. This is useful for building a master loot table from several independent sources (e.g., merging "Global Drops" and "Act 1 Rare Drops").

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#compositeTable)]

### Weight scaling

The [CompositeTableBuilder<T>](xref:NS.UnifiedLoot.CompositeTableBuilder`1) ensures that the internal relative weights within each sub-table are preserved. The effective weight of an entry inside the final composite is:

```
effectiveWeight = (selectionWeight / totalSelectionWeight) * (entryWeight / tableEntryWeight)
```

This ensures that a sub-table with higher `selectionWeight` contributes more items to the composite, without distorting the internal rarity of its items.

## [IntRange](xref:NS.UnifiedLoot.IntRange)

[IntRange](xref:NS.UnifiedLoot.IntRange) is a serialisable min/max quantity:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#intRange)]

## Custom Tables

Any class implementing [ILootTable<T>](xref:NS.UnifiedLoot.ILootTable`1) works. Use the
[LootTableIdGenerator](xref:NS.UnifiedLoot.LootTableIdGenerator) to get a unique auto-incremented int ID:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/TablesExamples.cs#customTable)]
