# Pity Systems

Pity systems guarantee players receive a desired outcome after enough failed attempts, preventing streaks of bad luck. Unified Loot provides two pity strategies, categorised by what they track:

| Target | Strategy | Behaviour |
|--------|----------|-----------|
| **Table** | `PityStrategy<T>` | **Hard pity** — guaranteed drop from the table after N failures |
| **Item** | `ItemPityStrategy<T>` | **Hard pity** for a specific item (e.g. "Epic Sword") |

All pity strategies use an `IPityTracker` to maintain state and support **group keys** for shared counters across multiple tables.

---

## IPityTracker — Persistent Pity State

> [!IMPORTANT]
> By default, pity strategies use an in-memory `PityTracker`. To persist pity counts across game sessions (save/load), provide your own implementation of `IPityTracker` to the strategy constructor.

```csharp
// Use your own persistent tracker (e.g. one that saves to JSON/PlayerPrefs)
var persistentTracker = new MyPersistentPityTracker();
var pity = new PityStrategy<Item>(threshold: 89, tracker: persistentTracker);
```

---

## Table-based Pity

Table-based pity strategies increment their failure counter whenever a roll against a table produces NO results.

### PityStrategy\<T\> — Hard Pity

After `threshold` consecutive rolls that produce no result, the next roll is guaranteed to succeed (one item is selected and added to the results).

```csharp
var pity = new PityStrategy<Item>(threshold: 89);

pipeline.AddStrategy(new WeightedRandomStrategy<Item>(1))
        .AddStrategy(pity);
```

#### How it works

1. If the result list is already non-empty, the failure counter resets to 0.
2. If the result list is empty **and** `failureCount >= threshold`, pity fires: one item is selected from the current table and added; failure counter resets.
3. Otherwise, failure counter increments.


## Item-based Pity

Item-based pity tracks failures for **specific items** across any table they might appear in. This is ideal for "rare chase items" that can drop from multiple sources.

### ItemPityStrategy\<T\>

Guarantees a specific item drops after it has failed to drop N times.

```csharp
// Requires an ID extractor to identify items
var itemPity = new ItemPityStrategy<Item>(item => item.Id);

// Add items to track
itemPity.AddTrackedItem(legendarySword, threshold: 100);
itemPity.AddTrackedItem(epicAxe, threshold: 50);

pipeline.AddStrategy(itemPity);
```


## Group Keys (Table Pity Only)

By default, each table is tracked separately by its `Id`. 

> [!NOTE]
> A **group key** makes multiple tables share the same counter — useful when a player can roll against different tables but you want a single pity guarantee across all of them.

```csharp
// All tables using groupKey: 42 share the same failure counter
var pity = new PityStrategy<Item>(threshold: 89, groupKey: 42);
```

---

## Querying and Resetting

All pity strategies implement `IResettable`.

```csharp
// Table-based: query by table ID or group key
int failures = pity.GetFailureCount(table.Id);
pity.Reset(table.Id);

// Item-based: query by item instance
int failures = itemPity.GetFailures(legendarySword);
itemPity.Reset(legendarySword);

// Reset everything
pity.ResetAll();
```
