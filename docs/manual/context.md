# Context System

The context system lets you pass runtime data, player stats, difficulty, flags to strategies
without coupling strategies to your game's specific types.

## [Key<T>](xref:NS.UnifiedLoot.Key`1)

A [Key<T>](xref:NS.UnifiedLoot.Key`1) is a lightweight typed token. Declare them as static fields so they are
created once and reused:

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#contextKeys)]

Each [Key<T>](xref:NS.UnifiedLoot.Key`1) gets a unique auto-incremented `int` ID at creation time.

## [Context](xref:NS.UnifiedLoot.Context)

[Context](xref:NS.UnifiedLoot.Context) is a simple `Dictionary<object, object>` wrapper with a fluent API.

### Building a context

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#buildingContext)]

### Reading values in a strategy

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#readingContext)]

### Mutating context

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#mutatingContext)]

### Passing context to Execute

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#passingContext)]

### Key reuse across pipelines

> [!TIP]
> A single context instance can serve multiple pipelines at once. Because keys are global,
> the same key type retrieves the same value from any context that has it set.

## Example: context-driven strategy

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ContextExamples.cs#contextDrivenStrategy)]