# Item Factories

Loot tables often store **definitions** templates that describe an item category (name, stat
ranges, rarity) rather than actual game objects. A factory converts those definitions into
**instances**, concrete items with rolled stats that go into the player's inventory.

## The Pattern

```
Table          Pipeline          Factory          Inventory
───────        ─────────         ───────          ─────────
LootTable<Def>  → Execute()  →  ILootFactory  →  List<Instance>
(Def, weight)     results       Create(Def)      (Instance, qty)
```

## [ILootFactory<TDefinition, TInstance>](xref:NS.UnifiedLoot.ILootFactory`2)

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/FactoriesExamples.cs#weaponFactory)]

## [ExecuteAndBuild](xref:NS.UnifiedLoot.LootPipeline`1.ExecuteAndBuild*)

[ExecuteAndBuild](xref:NS.UnifiedLoot.LootPipeline`1.ExecuteAndBuild*) is a method on [LootPipeline<T>](xref:NS.UnifiedLoot.LootPipeline`1). It runs the pipeline and
passes each result through the factory:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/FactoriesExamples.cs#executeAndBuild)]

### [BuiltLootResult<TDefinition, TInstance>](xref:NS.UnifiedLoot.BuiltLootResult`2)

The result type returned by [ExecuteAndBuild](xref:NS.UnifiedLoot.LootPipeline`1.ExecuteAndBuild*):

```csharp
public struct BuiltLootResult<TDefinition, TInstance> {
    public TDefinition Definition { get; } // original table entry
    public TInstance Instance { get; } // factory-created object
    public int Quantity { get; }
    public LootMetadata Metadata { get; }
}
```

## Example: ScriptableObject Definitions

A common Unity pattern is storing definitions as ScriptableObjects and using a factory to
instantiate runtime wrappers:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/FactoriesExamples.cs#scriptableObjectFactory)]

### Usage

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/FactoriesExamples.cs#scriptableUsage)]

> [!TIP]
> **One factory per item category** — keep factories small and focused.
> **Use context for scaling** — pass player level, difficulty, etc. to the factory so instances
> are appropriate to the current game state.

> [!NOTE]
> **Factory doesn't need to know about loot** — it only converts one object type to another;
> keep it free of loot system dependencies.
