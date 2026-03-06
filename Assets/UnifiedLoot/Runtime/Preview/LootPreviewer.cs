using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Preview {
    /// <summary>
    /// Utility for calculating the "dry run" state of a loot table processed by a pipeline.
    /// Useful for showing users what a loot table looks like after modifications.
    /// </summary>
    public static class LootPreviewer {
        /// <summary>
        /// Calculates a preview of the loot table after it has been processed by the pipeline's strategies.
        /// Note: This only accounts for strategies that modify weights/quantities globally (like ModifyWeightStrategy).
        /// Strategies that filter or generate results based on randomness (like WeightedRandomStrategy) are partially
        /// reflected in weight changes, but not as specific "results".
        /// </summary>
        public static LootTablePreview<T> GetPreview<T>(LootPipeline<T> pipeline, ILootTable<T> table, Context? context = null) {
            context ??= LootPipeline.EmptyContext;
            var preview = new LootTablePreview<T>();
            
            // 1. Initial State
            var workingSet = new LootWorkingSet<T>();
            workingSet.SourceTable = table;
            
            // Build initial weighted entries (similar to LootPipeline.BuildWeightedEntries)
            var cumulative = 0f;
            for (int i = 0; i < table.Count; i++) {
                var entry = table[i];
                cumulative += entry.Weight;
                workingSet.WeightedEntries.Add(new WeightedEntry<T> {
                    Entry = entry,
                    Index = i,
                    Weight = entry.Weight,
                    CumulativeWeight = cumulative
                });
                
                preview.Entries.Add(new LootPreviewEntry<T> {
                    Item = entry.Item,
                    OriginalWeight = entry.Weight,
                    ModifiedWeight = entry.Weight,
                    OriginalQuantity = entry.Quantity,
                    ModifiedQuantity = entry.Quantity,
                    OriginalIndex = i
                });
            }
            workingSet.TotalWeight = cumulative;

            // 2. Simulate Strategies
            foreach (var strategy in pipeline.Strategies) {
                if (strategy is ILootTableModifierStrategy<T>) {
                    strategy.Process(workingSet, context);
                } else if (strategy is ILootResultModifierStrategy<T> resultModifier) {
                    resultModifier.OnPreview(preview, context);
                }
                // ILootGeneratorStrategy are skipped
            }

            // 3. Finalize Preview
            preview.TotalWeight = workingSet.TotalWeight;
            
            // If the number of entries changed (e.g. a strategy added/removed table entries),
            // we should probably sync them. For now, we update weights for existing ones
            // and we could add new ones if needed.
            
            // Re-sync preview entries with working set weighted entries
            // This handles cases where strategies might have added or removed entries.
            var updatedEntries = new List<LootPreviewEntry<T>>();
            foreach (var we in workingSet.WeightedEntries) {
                // Try to find if we already have a preview entry for this original index
                var existing = preview.Entries.Find(e => e.OriginalIndex == we.Index);
                
                if (existing != null) {
                    existing.ModifiedWeight = we.Weight;
                    existing.Probability = preview.TotalWeight > 0 ? we.Weight / preview.TotalWeight : 0;
                    updatedEntries.Add(existing);
                } else {
                    // This is a new entry added by a strategy
                    updatedEntries.Add(new LootPreviewEntry<T> {
                        Item = we.Entry.Item,
                        OriginalWeight = 0, // It's new
                        ModifiedWeight = we.Weight,
                        OriginalQuantity = we.Entry.Quantity,
                        ModifiedQuantity = we.Entry.Quantity,
                        Probability = preview.TotalWeight > 0 ? we.Weight / preview.TotalWeight : 0,
                        OriginalIndex = we.Index
                    });
                }
            }
            
            preview.Entries.Clear();
            preview.Entries.AddRange(updatedEntries);

            return preview;
        }
    }
}
