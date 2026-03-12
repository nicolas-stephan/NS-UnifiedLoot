# Advanced

## Performance

### Avoiding result list allocations

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#avoidingAllocations)]

### Disabling metadata

[Metadata](xref:NS.UnifiedLoot.LootResult`1.Metadata) ([LootMetadata](xref:NS.UnifiedLoot.LootMetadata)) is collected by default. When you don't need debug info (production
hot paths), disable it:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#disablingMetadata)]

This skips filling [OriginalWeight](xref:NS.UnifiedLoot.LootMetadata.OriginalWeight), [FinalWeight](xref:NS.UnifiedLoot.LootMetadata.FinalWeight),
[RollValue](xref:NS.UnifiedLoot.LootMetadata.RollValue), and [SourceTableId](xref:NS.UnifiedLoot.LootMetadata.SourceTableId) on each
result.

---

## Deterministic Rolls

Use `SystemRandom` with a seed for reproducible results (testing, replays, server-authoritative
loot):

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#deterministicRolls)]

> [!NOTE]
> `UnityRandom` (the default) wraps `UnityEngine.Random` and is **not** seed-controlled per
> instance — it uses Unity's global random state.

---

## The Blackboard

[LootWorkingSet<T>.Blackboard](xref:NS.UnifiedLoot.LootWorkingSet`1.Blackboard) is a [Context](xref:NS.UnifiedLoot.Context) shared among all strategies
in a single roll. It is cleared between executions.

Use it when one strategy needs to communicate something to a later strategy without coupling
them. Like the context system, it uses type-safe keys:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#blackboard)]

---

## Pipeline Introspection

Inspect and modify the strategy list at runtime:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#introspection)]

---

## Custom Random

Implement [IRandom](xref:NS.UnifiedLoot.IRandom) to plug in any random source (e.g. a deterministic noise function,
server-provided seed, or crypto random):

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#customRandom)]

---

## Extending the System

### Custom table type

Implement [ILootTable<T>](xref:NS.UnifiedLoot.ILootTable`1). Use [LootTableIdGenerator.GetNextId()](xref:NS.UnifiedLoot.LootTableIdGenerator.GetNextId*)
for a unique `Id`.

### Custom entry type

Implement [ILootEntry<T>](xref:NS.UnifiedLoot.ILootEntry`1) — just [Item](xref:NS.UnifiedLoot.ILootEntry`1.Item), [Weight](xref:NS.UnifiedLoot.ILootEntry`1.Weight), and [Quantity](xref:NS.UnifiedLoot.ILootEntry`1.Quantity).

### Stateful strategies

If your strategy maintains state across rolls (like pity counters), implement [IResettable](xref:NS.UnifiedLoot.IResettable):

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#statefulStrategy)]

### Custom observer cleanup

Observers are held by reference. Remove them explicitly when the observing object is destroyed:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/AdvancedExamples.cs#observerCleanup)]
