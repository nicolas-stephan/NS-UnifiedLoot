# Observers

Observers let you react to completed rolls without modifying the pipeline itself. Common uses
include analytics, logging, achievement tracking, and UI notifications.

[OnRollComplete](xref:NS.UnifiedLoot.ILootObserver`1.OnRollComplete*) is called **after all strategies have run**, with the final result list.

## Registering Observers

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ObserversExamples.cs#registerObserver)]

Multiple observers are called in registration order.

## Example: Analytics

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ObserversExamples.cs#analyticsObserver)]

## Example: Debug Logger

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ObserversExamples.cs#debugLoggerObserver)]

## Example: Achievement Tracker

[!code-csharp[](../../Assets/UnifiedLoot/Samples~/Doc/ObserversExamples.cs#achievementObserver)]

## Considerations

> [!CAUTION]
> **Observers do not receive the Blackboard** — it is cleared when the working set is returned
> to the pool before observers are notified.

> [!IMPORTANT]
> **Observers receive `IReadOnlyList`** — the results list is read-only. To modify results, use
> a strategy instead.

> [!NOTE]
> **Execution order** — observers are called in registration order after all strategies finish.