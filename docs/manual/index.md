# Getting Started

This guide walks you from installation to your first loot roll in five minutes.

## Installation

Open your project's `Packages/manifest.json` and add the package:

```json
{
  "dependencies": {
    "com.ns.unified-loot": "https://github.com/nicolas-stephan/UnifiedLoot.git"
  }
}
```

To pin a specific version:

```json
"com.ns.unified-loot": "https://github.com/nicolas-stephan/UnifiedLoot.git#v0.2.0"
```

Unity will download the package automatically on next editor start.

> [!IMPORTANT]
> **Requirements:** Unity 6000.0+

## Core Concepts

Before jumping to code, understand the two key roles:

| Role         | Class             | Defines                                          |
|--------------|-------------------|--------------------------------------------------|
| **Table**    | [LootTable<T>](xref:NS.UnifiedLoot.LootTable`1)    | *What* can drop and at what weight               |
| **Pipeline** | [LootPipeline<T>](xref:NS.UnifiedLoot.LootPipeline`1) | *How* items are selected, filtered, and modified |

The pipeline never knows or cares which table it runs against. You can pass different tables to
the same pipeline at any time.

## Your First Table

Pick any type for your items — an enum is simplest to start:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/GettingStartedExamples.cs#firstTable)]

> [!NOTE]
> Weights are relative — they do not need to sum to 100.

## Your First Pipeline

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/GettingStartedExamples.cs#firstPipeline)]

[WeightedRandomStrategy](xref:NS.UnifiedLoot.WeightedRandomStrategy`1) is the most common starting point. It picks `rollCount` items from the
table using weighted random selection.

## Rolling

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/GettingStartedExamples.cs#roll)]

[Execute](xref:NS.UnifiedLoot.LootPipeline`1.Execute*) populates a `List<LootResult<T>>`. Each [LootResult<T>](xref:NS.UnifiedLoot.LootResult`1) has:

| Property                                               | Type           | Description                                                                     |
|--------------------------------------------------------|----------------|---------------------------------------------------------------------------------|
| [Item](xref:NS.UnifiedLoot.LootResult`1.Item)          | `T?`           | The dropped item (nullable — see [null entries](tables.md#null-items)) |
| [Quantity](xref:NS.UnifiedLoot.LootResult`1.Quantity)  | `int`          | How many dropped                                                                |
| [Metadata](xref:NS.UnifiedLoot.LootResult`1.Metadata)  | [LootMetadata](xref:NS.UnifiedLoot.LootMetadata) | Debug info: source table, weights, roll value                                   |

## Adding a Guaranteed Fallback

If all weighted rolls somehow produce nothing, [GuaranteedDropStrategy<T>](xref:NS.UnifiedLoot.GuaranteedDropStrategy`1) makes sure at least
one item always drops:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/GettingStartedExamples.cs#guaranteedPipeline)]

## Reusing the Pipeline

Pipelines are **stateless by default** (stateful strategies like [PityStrategy<T>](xref:NS.UnifiedLoot.PityStrategy`1) are the
exception). Create them once and call [[Execute](xref:NS.UnifiedLoot.LootPipeline`1.Execute*)] as many times as you need:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/GettingStartedExamples.cs#reusingPipeline)]

## Next Steps

- [Tables](tables.md) - LootTable, ScriptableObject tables, CompositeTable
- [Context System](context.md) - pass runtime data to strategies
- [Strategies](strategies.md) - all built-in strategies and how to write your own
- [Preview System](preview.md) - dry-run loot tables in editor or code
- [Pity Systems](pity-systems.md) - hard and soft pity
- [Factories](factories.md) - roll definitions, instantiate instances
