# Strategies

Strategies are the building blocks of a pipeline. Each implements [ILootStrategy<T>](xref:NS.UnifiedLoot.ILootStrategy`1) and
receives the working set (mutable roll state) and context (read-only caller data).

```csharp
public interface ILootStrategy<T> {
    void Process(LootWorkingSet<T> workingSet, LootContext context);
}
```

Strategies are executed in the order they are added to the pipeline.

## Strategy Ordering

> [!IMPORTANT]
> Order matters. A typical pipeline flows like this:
> 
> ```
> [ModifyWeight]  →  [Selection]  →  [Pity]  →  [Filter]  →  [PostProcess]
> ```

| Phase | Strategies |
|-------|-----------|
| Pre-roll weight scaling | [ModifyWeightStrategy<T>](xref:NS.UnifiedLoot.ModifyWeightStrategy`1) |
| Selection | [WeightedRandomStrategy<T>](xref:NS.UnifiedLoot.WeightedRandomStrategy`1), [DropChanceStrategy<T>](xref:NS.UnifiedLoot.DropChanceStrategy`1) |
| Guarantee / fallback | [GuaranteedDropStrategy<T>](xref:NS.UnifiedLoot.GuaranteedDropStrategy`1), [PityStrategy<T>](xref:NS.UnifiedLoot.PityStrategy`1), [SoftPityStrategy<T>](xref:NS.UnifiedLoot.SoftPityStrategy`1), [ItemPityStrategy<T>](xref:NS.UnifiedLoot.ItemPityStrategy`1), [SoftItemPityStrategy<T>](xref:NS.UnifiedLoot.SoftItemPityStrategy`1) |
| Filtering | [FilterStrategy<T>](xref:NS.UnifiedLoot.FilterStrategy`1), [FilterByContextStrategy<T>](xref:NS.UnifiedLoot.FilterByContextStrategy`1) |
| Post-processing | [ModifyQuantityStrategy<T>](xref:NS.UnifiedLoot.ModifyQuantityStrategy`1), [LimitResultsStrategy<T>](xref:NS.UnifiedLoot.LimitResultsStrategy`1), [ConsolidateResultsStrategy<T>](xref:NS.UnifiedLoot.ConsolidateResultsStrategy`1), [ExpandResultsStrategy<T>](xref:NS.UnifiedLoot.ExpandResultsStrategy`1), [BonusRollStrategy<T>](xref:NS.UnifiedLoot.BonusRollStrategy`1), [UniqueDropStrategy<T>](xref:NS.UnifiedLoot.UniqueDropStrategy`1), [NestedTableStrategy<T>](xref:NS.UnifiedLoot.NestedTableStrategy`1) |

---

## Selection Strategies

### [WeightedRandomStrategy<T>](xref:NS.UnifiedLoot.WeightedRandomStrategy`1)

Picks `rollCount` items from the table using cumulative weighted random selection.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#weightedRandom)]

> [!NOTE]
> The same item can appear multiple times in the results if `allowDuplicates` is `true` (default).

### [DropChanceStrategy<T>](xref:NS.UnifiedLoot.DropChanceStrategy`1)

Rolls every entry **independently**. Each entry's `Weight` is treated as a probability, not a
relative weight.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#dropChance)]

> [!TIP]
> This is useful for tables where each entry should be evaluated separately rather than competing
> with each other.

---

## Guarantee / Fallback Strategies

### [GuaranteedDropStrategy<T>](xref:NS.UnifiedLoot.GuaranteedDropStrategy`1)

Only activates when the result list is empty. Performs one weighted random selection from the
table.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#guaranteedDrop)]

> [!TIP]
> Place this **after** your selection strategies so it only fires when they produce nothing.

---

## Pity Strategies

See [Pity Systems](pity-systems.md) for full details.

### [PityStrategy<T>](xref:NS.UnifiedLoot.PityStrategy`1)

Hard pity: after `threshold` consecutive rolls that produce no result, the next roll **always**
succeeds. Resets on success. Tracks by table ID or group key.

### [SoftPityStrategy<T>](xref:NS.UnifiedLoot.SoftPityStrategy`1)

Soft pity: linearly ramps bonus drop chance from `softPityStart` failures to a guarantee at
`hardPityAt`. Tracks by table ID or group key.

### [ItemPityStrategy<T>](xref:NS.UnifiedLoot.ItemPityStrategy`1)

Hard pity for a specific item. Guarantees the item drops after it has failed to drop N times.

### [SoftItemPityStrategy<T>](xref:NS.UnifiedLoot.SoftItemPityStrategy`1)

Soft pity for a specific item. Probabilistic chance increases linearly with failures.

---

## Pre-roll Modification

### [ModifyWeightStrategy<T>](xref:NS.UnifiedLoot.ModifyWeightStrategy`1)

> [!IMPORTANT]
> Scale entry weights **before** selection. Must be placed before any selection strategy.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#modifyWeight)]

> [!IMPORTANT]
> Recalculates cumulative weights and `TotalWeight` after modification so selection strategies see the updated distribution.

---

## Filtering Strategies

### [FilterStrategy<T>](xref:NS.UnifiedLoot.FilterStrategy`1)

Removes results where the predicate returns `false`.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#filter)]

### [FilterByContextStrategy<T>](xref:NS.UnifiedLoot.FilterByContextStrategy`1)

Like [FilterStrategy<T>](xref:NS.UnifiedLoot.FilterStrategy`1) but the predicate also receives the context:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#filterByContext)]

---

## Quantity Modification

### [ModifyQuantityStrategy<T>](xref:NS.UnifiedLoot.ModifyQuantityStrategy`1)

Adjusts the quantity of each result after selection.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#modifyQuantity)]

---

## Result Count Strategies

### [LimitResultsStrategy<T>](xref:NS.UnifiedLoot.LimitResultsStrategy`1)

Caps the result list at `maxResults`. Excess items are removed from the end.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#limitResults)]

### [LimitResultsFromContextStrategy<T>](xref:NS.UnifiedLoot.LimitResultsFromContextStrategy`1)

Reads the limit from a context key:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#limitResultsFromContext)]

---

## Post-processing Strategies

### [ConsolidateResultsStrategy<T>](xref:NS.UnifiedLoot.ConsolidateResultsStrategy`1)

Merges duplicate items into a single result by summing their quantities.

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/StrategiesExamples.cs#consolidateResults)]

### [BonusRollStrategy<T>](xref:NS.UnifiedLoot.BonusRollStrategy`1)

Reads a 0–1 chance from the context and adds one extra weighted random roll if the chance
succeeds.

```csharp
// LootKeys.Luck = 1.0 → 100% bonus roll; 0.0 → no bonus roll
.AddStrategy(new BonusRollStrategy<Item>(LootKeys.Luck))
```

### [UniqueDropStrategy<T>](xref:NS.UnifiedLoot.UniqueDropStrategy`1)

Stateful. Tracks items that have already dropped (per instance) and removes duplicates from
future results.

```csharp
var uniqueDrops = new UniqueDropStrategy<Item>();

// Later — reset between sessions or dungeon runs
uniqueDrops.ResetAll();   // or uniqueDrops.Reset(specificItem)

// Check / manually mark
uniqueDrops.HasDropped(sword);      // bool
uniqueDrops.MarkAsDropped(sword);   // manual override
```

### [NestedTableStrategy<T>](xref:NS.UnifiedLoot.NestedTableStrategy`1)

When an item acts as a "table selector", resolve it against a sub-table and replace the item
with the results of that sub-table.

```csharp
// Provide a resolver that maps an item to its sub-table
.AddStrategy(new NestedTableStrategy<TableKey>(
    tableResolver: key => tableRegistry[key],
    nestedPipeline: innerPipeline // optional; defaults to WeightedRandom(1)
))
```

> [!NOTE]
> Items that don't resolve to a table (resolver returns null) are kept as-is.

### [ExpandResultsStrategy<T>](xref:NS.UnifiedLoot.ExpandResultsStrategy`1)

Expands individual results into multiple items via a delegate. Useful for "loot set" items that
contain a collection of actual drops.

```csharp
.AddStrategy(new ExpandResultsStrategy<Item>(
    expander: item => lootSetDatabase[item], // returns IEnumerable<Item>? or null to skip
    quantityResolver: item => new IntRange(1, 3) // optional per-expanded-item quantity
))
```

> [!NOTE]
> Items where the expander returns `null` are kept unchanged.

---

## Writing a Custom Strategy

Implement `ILootStrategy<T>`. You have full access to the working set's results, weighted
entries, blackboard, random, and source table:

```csharp
public class ElementalFilterStrategy<T> : ILootStrategy<T> where T : IElemental {
    readonly Element _allowedElement;

    public ElementalFilterStrategy(Element allowedElement)
        => _allowedElement = allowedElement;

    public void Process(LootWorkingSet<T> workingSet, LootContext context) {
        for (var i = workingSet.Results.Count - 1; i >= 0; i--) {
            var item = workingSet.Results[i].Item;
            if (item != null && item.Element != _allowedElement)
                workingSet.Results.RemoveAt(i);
        }
    }
}
```

### Accessing the random source

```csharp
float roll = workingSet.Random.Value; // 0–1 exclusive
int idx  = workingSet.Random.Range(0, 5); // exclusive upper bound
```

### Inter-strategy communication via Blackboard

```csharp
public static class MyKeys {
    public static readonly BlackboardKey<int> RollCount = new("RollCount");
}

// Strategy A — store data
workingSet.Blackboard.Set(MyKeys.RollCount, rollCount);

// Strategy B — read data
if (workingSet.Blackboard.TryGet(MyKeys.RollCount, out int n))
    // use n
```

The blackboard is a `LootBlackboard` cleared between rolls. Use `BlackboardKey<T>` for type-safe access.

### Pipeline introspection

```csharp
IReadOnlyList<ILootStrategy<T>> steps = pipeline.Strategies;

// Insert before an existing strategy
int idx = pipeline.Strategies.IndexOf(existingStrategy);
pipeline.InsertStrategy(idx, myNewStrategy);

// Remove a strategy
pipeline.RemoveStrategy(existingStrategy);
```
