---
_disableToc: false
---

# UnifiedLoot documentation

A type-safe, composable loot resolution engine for Unity.

Designed for games with high-frequency loot operations — think Path of Exile, Diablo, or Borderlands. Build flexible loot systems by composing strategies into pipelines, then roll against any table on demand.

## At a Glance

- **Pipeline Driven**: Chain strategies like filtering, weighted random, and pity into a reusable pipeline.
- **Type-Safe**: Use any type for your items — Enums, ScriptableObjects, or custom classes.
- **Context Aware**: Pass runtime data (Luck, Player Level, etc.) to influence roll outcomes.
- **Performant**: Minimal allocations, object pooling, and efficient data structures.

<div class="row g-4 mb-4">
    <div class="col-md-6">
        <div class="card h-100">
            <div class="card-body">
                <h2 class="card-title h5">📚 Manual</h2>
                <p class="card-text">Step-by-step guides to get you started, from installation to advanced concepts like pity systems and context-based modifiers.</p>
            </div>
            <p class="px-3 mb-4"><a class="stretched-link" href="manual/index.md">Explore Manual</a></p>
        </div>
    </div>
    <div class="col-md-6">
        <div class="card h-100">
            <div class="card-body">
                <h2 class="card-title h5">🎓 Tutorials</h2>
                <p class="card-text">Hands-on tutorials for common game scenarios like boss drops, gacha banners, and dynamic loot scaling.</p>
            </div>
            <p class="px-3 mb-4"><a class="stretched-link" href="tutorials/index.md">View Tutorials</a></p>
        </div>
    </div>
    <div class="col-md-6">
        <div class="card h-100">
            <div class="card-body">
                <h2 class="card-title h5">🔧 API Reference</h2>
                <p class="card-text">Detailed technical documentation for all classes, methods, and properties in the UnifiedLoot package.</p>
            </div>
            <p class="px-3 mb-4"><a class="stretched-link" href="api/index.md">Go to API Reference</a></p>
        </div>
    </div>
</div>