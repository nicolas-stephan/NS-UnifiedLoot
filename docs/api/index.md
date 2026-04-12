# UnifiedLoot API Reference

This section contains the automatically generated API documentation for the UnifiedLoot package. All components reside under the `NS.UnifiedLoot` namespace.

### Core Components
- [Core Namespace](NS.UnifiedLoot.yml): The fundamental building blocks, including `Context`, `LootPipeline`, and `LootResult`.
- [Loot Factories](NS.UnifiedLoot.ILootFactory-2.yml): Interfaces and implementations for creating item instances from definitions.

### Tables & Data
- [Loot Tables](NS.UnifiedLoot.ILootTable-1.yml): Definitions for loot tables, entries, and composite table builders.
- [Scriptable Assets](NS.UnifiedLoot.LootTableAssetBase.yml): Unity ScriptableObject wrappers for loot tables.

### Strategies
- [Loot Strategies](NS.UnifiedLoot.ILootStrategy-1.yml): The core interface for all selection, modification, and filtering logic.
- [Built-in Strategies](NS.UnifiedLoot.yml): Selection (Weighted, Drop Chance), Modification (Quantity, Weight), and Filtering.

### Systems
- [Pity System](NS.UnifiedLoot.IPityTracker.yml): Trackers and strategies for guaranteed drops and soft-pity logic.
- [Randomization](NS.UnifiedLoot.IRandom.yml): Abstractions for random number generation (System and Unity).
- [Preview & Testing](NS.UnifiedLoot.LootPreviewer.yml): Tools for simulating and inspecting loot table outputs.
