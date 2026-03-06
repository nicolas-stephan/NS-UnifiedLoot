# Unified Loot

A type-safe, composable loot resolution engine for Unity. Designed for games
with high-frequency loot operations — think Path of Exile, Diablo, or Borderlands.

## Features

- **Pipeline Architecture** — Compose strategies into a reusable processing chain
- **Type-Safe Generics** — Bring your own item type: enum, struct, class, ScriptableObject
- **Flexible Context** — Pass any runtime data to strategies via typed, int-keyed context keys
- **High Performance** — Object pooling, optional metadata, int IDs over strings
- **Multiple Table Types** — Code-defined, ScriptableObject, or composite (merged sub-tables)
- **Pity Systems** — Hard pity (`PityStrategy`) and soft pity (`SoftPityStrategy`) with group keys
- **Observer Pattern** — Hook into roll completions for analytics and logging
- **Resettable Strategies** — `IResettable` interface for session resets
- **Editor Tooling** — Custom inspector with probability summary and live validation
- **Networking Ready** — Built-in NGO support via `#if UNIFIED_LOOT_NGO`

## Installation

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ns.unified-loot": "https://github.com/nicolas-stephan/UnifiedLoot.git"
  }
}
```

Or pin a specific version tag:

```json
"com.ns.unified-loot": "https://github.com/nicolas-stephan/UnifiedLoot.git#v0.2.0"
```

**Requirements:** Unity 6000.0+

## Quick Start

```csharp
using NS.UnifiedLoot;

// 1. Define your item type (enum, ScriptableObject, etc.)
public enum ItemRarity { Common, Rare, Legendary }

// 2. Create a loot table
var table = new LootTable<ItemRarity>("ChestDrops")
    .Add(ItemRarity.Common,    weight: 70f)
    .Add(ItemRarity.Rare,      weight: 25f)
    .Add(ItemRarity.Legendary, weight: 5f);

// 3. Build a pipeline once and reuse it
var pipeline = new LootPipeline<ItemRarity>()
    .AddStrategy(new WeightedRandomStrategy<ItemRarity>(rollCount: 1))
    .AddStrategy(new GuaranteedDropStrategy<ItemRarity>());

// 4. Roll!
var results = pipeline.Execute(table);
foreach (var result in results)
    Debug.Log($"Got: {result.Item} x{result.Quantity}");
```

## Context System

Pass runtime data to strategies without coupling them to your game logic:

```csharp
// Define typed keys (create once, store as statics)
public static class LootKeys
{
    public static readonly ContextKey<float> Luck       = new("Luck");
    public static readonly ContextKey<int>   PlayerLevel = new("PlayerLevel");
    public static readonly ContextKey<bool>  IsBoss      = new("IsBoss");
}

// Build pipeline using context
var pipeline = new LootPipeline<ItemDef>()
    .AddStrategy(new WeightedRandomStrategy<ItemDef>(3))
    .AddStrategy(new BonusRollStrategy<ItemDef>(LootKeys.Luck))
    .AddStrategy(new ModifyWeightStrategy<ItemDef>(
        ModifyWeightStrategy<ItemDef>.ScaleByLevelRange(LootKeys.PlayerLevel, minLevel: 1, maxLevel: 60)));

// Roll with context
var context = new LootContext()
    .With(LootKeys.Luck, 1.5f)
    .With(LootKeys.PlayerLevel, 42);

var results = pipeline.Execute(table, context);
```

## Built-in Strategies

### Selection

| Strategy | Description |
|----------|-------------|
| `WeightedRandomStrategy<T>` | Picks N entries by relative weight; supports `allowDuplicates` |
| `DropChanceStrategy<T>` | Rolls each entry independently against its weight as a drop chance |
| `GuaranteedDropStrategy<T>` | Fallback: ensures at least one result when the list is empty |

### Pity

| Strategy | Description |
|----------|-------------|
| `PityStrategy<T>` | Hard pity: guarantees a drop after N consecutive failures |
| `SoftPityStrategy<T>` | Soft pity: linearly ramps bonus chance from a threshold up to a guarantee |

### Modification

| Strategy | Description |
|----------|-------------|
| `ModifyWeightStrategy<T>` | Scales entry weights before selection; use before selection strategies |
| `ModifyQuantityStrategy<T>` | Adjusts quantities after selection (multipliers, context-driven) |

### Filtering & Limiting

| Strategy | Description |
|----------|-------------|
| `FilterStrategy<T>` | Removes results matching a predicate |
| `FilterByContextStrategy<T>` | Like `FilterStrategy` but the predicate also receives the context |
| `LimitResultsStrategy<T>` | Caps the result count; `LimitResultsFromContextStrategy` reads the cap from context |

### Post-processing

| Strategy | Description |
|----------|-------------|
| `ConsolidateResultsStrategy<T>` | Merges duplicate items by summing quantities |
| `BonusRollStrategy<T>` | Adds an extra roll when a luck context value succeeds |
| `UniqueDropStrategy<T>` | Stateful — prevents the same item dropping more than once |
| `NestedTableStrategy<T>` | Resolves items as sub-tables; items become their resolved drops |
| `ExpandResultsStrategy<T>` | Expands items into collections via a delegate (e.g. "loot set" items) |

## Custom Strategies

Implement `ILootStrategy<T>` to add your own processing step:

```csharp
public class MinRarityFilter<T> : ILootStrategy<T> where T : IHasRarity
{
    readonly Rarity _min;
    public MinRarityFilter(Rarity min) => _min = min;

    public void Process(LootWorkingSet<T> workingSet, LootContext context)
    {
        for (var i = workingSet.Results.Count - 1; i >= 0; i--)
            if (workingSet.Results[i].Item?.Rarity < _min)
                workingSet.Results.RemoveAt(i);
    }
}
```

## Table Types

### Code-defined

```csharp
var table = new LootTable<Item>("BossDrops")
    .Add(sword,  weight: 10f)
    .Add(shield, weight: 10f)
    .Add(potion, weight: 80f, minQuantity: 2, maxQuantity: 5);
```

### ScriptableObject

```csharp
[CreateAssetMenu(menuName = "Loot/Item Table")]
public class ItemLootTable : LootTableAsset<ItemDefinition> { }
```

Create the asset in the Project window. The custom inspector shows a probability summary
and flags validation issues (null items, zero weights, single-entry tables).

### Composite

Combine multiple tables into a virtual flat table with weighted sub-table selection:

```csharp
var composite = new CompositeTable<Item>("WorldDrops")
    .Add(commonTable,    selectionWeight: 70f)
    .Add(rareTable,      selectionWeight: 25f)
    .Add(legendaryTable, selectionWeight: 5f);

var results = pipeline.Execute(composite, context);
```

## Item Factory

When your table contains **definitions** (stat templates) and you need **instances** (rolled items):

```csharp
public class WeaponFactory : ILootFactory<WeaponDef, WeaponInstance>
{
    public WeaponInstance Create(WeaponDef def, LootContext context)
    {
        var level = context.GetOrDefault(LootKeys.PlayerLevel, 1);
        return new WeaponInstance
        {
            Name   = def.Name,
            Damage = def.DamageRange.Roll(new SystemRandom()) + level,
            Rarity = def.Rarity
        };
    }
}

// ExecuteAndBuild rolls the table and passes each result through the factory
var built = pipeline.ExecuteAndBuild(weaponTable, new WeaponFactory(), context);
foreach (var r in built)
    inventory.Add(r.Instance, r.Quantity);  // r.Definition is also available
```

## Observers

Hook into roll completion without modifying the pipeline:

```csharp
public class LootAnalytics : ILootObserver<ItemDef>
{
    public void OnRollComplete(ILootTable<ItemDef> table,
                               IReadOnlyList<LootResult<ItemDef>> results,
                               LootContext context)
    {
        foreach (var r in results)
            Analytics.Track("item_dropped", r.Item?.Name, r.Quantity);
    }
}

pipeline.AddObserver(new LootAnalytics());
```

## Pity Systems

```csharp
// Hard pity: guaranteed drop after 89 failures, resets on success
var pity = new PityStrategy<Item>(threshold: 89);

// Soft pity: ramp bonus chance from 75 failures, guarantee at 90
var softPity = new SoftPityStrategy<Item>(softPityStart: 75, hardPityAt: 90);

// Group key: share a pity counter across multiple tables
var pity = new PityStrategy<Item>(threshold: 89, groupKey: 42);

// Reset all counters (e.g. on session end)
pity.ResetAll();   // implements IResettable
```

## Performance Tips

- Create `LootPipeline` and tables once; reuse them across many rolls
- Use `.WithMetadata(false)` to skip metadata collection in hot paths
- Use the `Execute(table, resultsList, context)` overload to avoid per-roll list allocations
- `ContextKey<T>` uses an auto-incremented `int` ID — zero string overhead
- `LootWorkingSet<T>` is pooled internally

## Documentation

Full documentation including guides, strategy reference, and examples:

**[nicolas-stephan.github.io/UnifiedLoot](https://nicolas-stephan.github.io/UnifiedLoot)**
