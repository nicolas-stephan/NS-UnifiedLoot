using System.Collections;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// A virtual table composed of multiple sub-tables, each with a selection weight.
    /// When enumerated, the pipeline sees a single flat list of entries; each entry's effective
    /// weight is scaled proportionally so that the sub-tables' selection weights are honored.
    ///
    /// <para>
    /// Example — 30 % weapons, 70 % armor:
    /// <code>
    /// var composite = new CompositeTable&lt;ItemDef&gt;()
    ///     .Add(weaponTable, 0.3f)
    ///     .Add(armourTable, 0.7f);
    /// </code>
    /// </para>
    /// </summary>
    public class CompositeTable<T> : ILootTable<T> {
        private struct SubTableInfo {
            public ILootTable<T> Table;
            public float SelectionWeight;
            public float TotalEntryWeight;
            public List<ILootEntry<T>> CachedEntries;
        }

        private readonly List<SubTableInfo> _subTables = new();
        private float _totalSelectionWeight;
        private int _cachedCount;

        public int Id { get; } = LootTableIdGenerator.GetNextId();
        public string? Name { get; }

        public int Count => _cachedCount;
        public ILootEntry<T> this[int index] {
            get {
                if (index < 0 || index >= _cachedCount)
                    throw new System.IndexOutOfRangeException();

                var currentOffset = 0;
                foreach (var subTable in _subTables) {
                    if (index < currentOffset + subTable.Table.Count) {
                        var subIndex = index - currentOffset;
                        var entry = subTable.Table[subIndex];
                        if (_totalSelectionWeight <= 0f || subTable.TotalEntryWeight <= 0f)
                            return new ScaledEntry(entry, 0f);

                        var tableProportion = subTable.SelectionWeight / _totalSelectionWeight;
                        var scaledWeight = tableProportion * (entry.Weight / subTable.TotalEntryWeight);
                        return new ScaledEntry(entry, scaledWeight);
                    }
                    currentOffset += subTable.Table.Count;
                }
                throw new System.IndexOutOfRangeException();
            }
        }

        public CompositeTable(string? name = null) => Name = name;

        public CompositeTable<T> Add(ILootTable<T> table, float weight) {
            var info = new SubTableInfo {
                Table = table,
                SelectionWeight = weight,
                CachedEntries = new List<ILootEntry<T>>()
            };

            foreach (var entry in table) {
                info.CachedEntries.Add(entry);
                info.TotalEntryWeight += entry.Weight;
            }

            _subTables.Add(info);
            _totalSelectionWeight += weight;
            _cachedCount += table.Count;
            return this;
        }

        public CompositeTable<T> AddMany<TTable>(IEnumerable<(TTable table, float weight)> tables) where TTable : ILootTable<T> {
            foreach (var (table, weight) in tables)
                Add(table, weight);

            return this;
        }

        public IEnumerator<ILootEntry<T>> GetEnumerator() {
            if (_totalSelectionWeight <= 0f)
                yield break;

            foreach (var subTable in _subTables) {
                if (subTable.TotalEntryWeight <= 0f)
                    continue;

                var tableProportion = subTable.SelectionWeight / _totalSelectionWeight;

                foreach (var entry in subTable.CachedEntries) {
                    // Scale the entry weight so that the sub-table occupies its correct slice
                    var scaledWeight = tableProportion * (entry.Weight / subTable.TotalEntryWeight);
                    yield return new ScaledEntry(entry, scaledWeight);
                }
            }
        }

        public void Clear() {
            _subTables.Clear();
            _totalSelectionWeight = 0f;
            _cachedCount = 0;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class ScaledEntry : ILootEntry<T> {
            private readonly ILootEntry<T> _inner;

            public T? Item => _inner.Item;
            public float Weight { get; }
            public IntRange Quantity => _inner.Quantity;

            public ScaledEntry(ILootEntry<T> inner, float weight) {
                _inner = inner;
                Weight = weight;
            }
        }
    }
}