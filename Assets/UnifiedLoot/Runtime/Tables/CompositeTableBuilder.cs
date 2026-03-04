using System;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// Builder for creating a flattened, immutable loot table from multiple sub-tables.
    /// Replaces the runtime CompositeTable.
    /// </summary>
    public sealed class CompositeTableBuilder<T> {
        private struct SubTableInfo {
            public ILootTable<T> Table;
            public float SelectionWeight;
        }

        private readonly List<SubTableInfo> _subTables = new();

        public CompositeTableBuilder<T> Add(ILootTable<T> table, float selectionWeight) {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            _subTables.Add(new SubTableInfo { Table = table, SelectionWeight = selectionWeight });
            return this;
        }

        public CompositeTableBuilder<T> AddMany(IEnumerable<(ILootTable<T> table, float weight)> tables) {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            foreach (var (table, weight) in tables) {
                Add(table, weight);
            }

            return this;
        }

        public ILootTable<T> Build() {
            var totalSelectionWeight = 0f;
            foreach (var sub in _subTables)
                totalSelectionWeight += sub.SelectionWeight;

            var flattenedEntries = new List<ILootEntry<T>>();
            if (totalSelectionWeight <= 0f)
                return new LootTable<T>(flattenedEntries);

            foreach (var sub in _subTables) {
                var table = sub.Table;
                var totalEntryWeight = 0f;

                for (var i = 0; i < table.Count; i++)
                    totalEntryWeight += table[i].Weight;

                if (totalEntryWeight <= 0f)
                    continue;

                var tableProportion = sub.SelectionWeight / totalSelectionWeight;

                for (var i = 0; i < table.Count; i++) {
                    var entry = table[i];
                    var scaledWeight = tableProportion * (entry.Weight / totalEntryWeight);
                    flattenedEntries.Add(new LootEntry<T>(entry.Item, scaledWeight, entry.Quantity));
                }
            }

            return new LootTable<T>(flattenedEntries);
        }
    }
}