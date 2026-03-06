using System;
using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using UnityEngine;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Tables {
    /// <summary>
    /// A ScriptableObject-based loot table for designer-defined loot.
    /// </summary>
    /// <typeparam name="TItem">
    /// The item type for entries in this table.
    /// <para>
    /// <b>Warning:</b> <typeparamref name="TItem"/> must be a Unity-serializable type —
    /// a primitive, an enum, a <c>[Serializable]</c> class/struct, or a
    /// <see cref="UnityEngine.Object"/> subclass. Unsupported types will not appear in the
    /// Inspector and will serialize as default/null.
    /// </para>
    /// </typeparam>
    public enum LootEntryType {
        Item,
        SubTable
    }

    public abstract class LootTableAsset<TItem> : LootTableAssetBase {
        public const string NameOfEntries = nameof(entries);

        [SerializeField] private List<LootEntryData> entries = new();
#if UNITY_EDITOR
        internal void AddEntry(TItem item, float weight = 1f, IntRange? quantity = null) {
            entries.Add(new LootEntryData(item, weight, quantity ?? 1));
            _cachedTable = null;
        }

        internal void AddEntry(LootTableAsset<TItem> subTable, float weight = 1f, IntRange? quantity = null) {
            entries.Add(new LootEntryData(subTable, weight, quantity ?? 1));
            _cachedTable = null;
        }

        internal void ClearEntries() {
            entries.Clear();
            _cachedTable = null;
        }

        internal void TriggerValidate() => OnValidate();
#endif

        private LootTable<TItem>? _cachedTable;
        private int _id;

        public int Id => _id != 0 ? _id : (_id = LootTableIdGenerator.GetNextId());

        public override int TableId => Id;
        public override int EntryCount => entries.Count;

        public override IEnumerable<LootTableAssetBase> GetSubTables() {
            if (entries == null) yield break;
            foreach (var entry in entries) {
                if (entry.EntryType == LootEntryType.SubTable && entry.SubTable != null) {
                    yield return entry.SubTable;
                }
            }
        }

        /// <summary>
        /// Flattens this asset into a runtime-optimized <see cref="ILootTable{TItem}"/>.
        /// This table is immutable and uses precomputed weights for performance.
        /// </summary>
        /// <returns>A runtime-ready loot table.</returns>
        public LootTable<TItem> ToTable() {
            if (_cachedTable != null)
                return _cachedTable;

            var stack = new List<LootTableAssetBase>();
            if (HasCircularDependency(stack)) {
                var chain = string.Join(" -> ", stack.ConvertAll(s => s != null ? s.name : "(null)"));
                throw new InvalidOperationException($"[UnifiedLoot] Circular dependency detected in table '{name}': {chain}. Flattening aborted.");
            }

            var builder = new CompositeTableBuilder<TItem>();
            foreach (var entry in entries) {
                if (entry.EntryType == LootEntryType.SubTable && entry.SubTable != null) {
                    builder.Add(entry.SubTable.ToTable(), entry.Weight, entry.Quantity);
                }
                else {
                    // We wrap the single item in a temporary table for the builder to flatten.
                    // The builder takes care of weight normalization across all entries.
                    var singleItemTable = new LootTable<TItem>().Add(new LootEntry<TItem>(entry.Item, 1f, entry.Quantity));
                    builder.Add(singleItemTable, entry.Weight);
                }
            }
            return _cachedTable = builder.Build();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate() {
            _cachedTable = null;

            var stack = new List<LootTableAssetBase>();
            if (HasCircularDependency(stack)) {
                var chain = string.Join(" -> ", stack.ConvertAll(s => s != null ? s.name : "(null)"));
                Debug.LogError($"[UnifiedLoot] Circular dependency detected in table '{name}': {chain}.", this);
            }

            if (entries.Count == 0) {
                // If it's empty, we don't warn yet as it might be a new asset.
                return;
            }

            if (entries.Count == 1) {
                Debug.LogWarning($"[UnifiedLoot] Table '{name}' has only one entry — weighted selection is degenerate.", this);
            }

            for (var i = 0; i < entries.Count; i++) {
                var entry = entries[i];
                if (entry.Weight <= 0f)
                    Debug.LogWarning($"[UnifiedLoot] Table '{name}' entry #{i} has weight <= 0.", this);
                
                if (entry.EntryType == LootEntryType.SubTable) {
                    if (entry.SubTable is null)
                        Debug.LogWarning($"[UnifiedLoot] Table '{name}' entry #{i} has a null sub-table reference.", this);
                }
                else {
                    if (entry.Item is null)
                        Debug.LogWarning($"[UnifiedLoot] Table '{name}' entry #{i} has a null item reference.", this);
                }
            }
        }
#endif

        /// <summary>
        /// Entry data stored in the asset.
        /// </summary>
        [Serializable]
        public class LootEntryData : LootEntryDataBase {
            public const string NameOfItem = nameof(item);
            public const string NameOfSubTable = nameof(subTable);
            public const string NameOfEntryType = nameof(entryType);

            [SerializeField] private LootEntryType entryType = LootEntryType.Item;
            [SerializeField] private TItem? item;
            [SerializeField] private LootTableAsset<TItem> subTable;

            public LootEntryType EntryType => entryType;
            public TItem? Item => item;
            public LootTableAsset<TItem> SubTable => subTable;
#if UNITY_EDITOR
            internal LootEntryData() { }

            internal LootEntryData(TItem item, float weight, IntRange quantity) {
                this.entryType = LootEntryType.Item;
                this.item = item;
                this.weight = weight;
                this.quantity = quantity;
            }

            internal LootEntryData(LootTableAsset<TItem> subTable, float weight, IntRange quantity) {
                this.entryType = LootEntryType.SubTable;
                this.subTable = subTable;
                this.weight = weight;
                this.quantity = quantity;
            }
#endif
        }
    }

    /// <summary>
    /// Non-generic base for <see cref="LootTableAsset{TItem}"/> entries.
    /// Targeted by the property drawer so that weight, quantity and note render
    /// with probability hints in the Inspector.
    /// </summary>
    [Serializable]
    public abstract class LootEntryDataBase {
        public const string NameOfWeight = nameof(weight);
        public const string NameOfQuantity = nameof(quantity);
        [SerializeField] protected float weight = 1f;
        [SerializeField] protected IntRange quantity = new(1);

        public float Weight => weight;
        public IntRange Quantity => (quantity.Min == 0 && quantity.Max == 0) ? new IntRange(1) : quantity;
    }

    /// <summary>
    /// Non-generic base for editor tooling. Not intended for direct use.
    /// </summary>
    public abstract class LootTableAssetBase : ScriptableObject {
        public abstract int TableId { get; }
        public abstract int EntryCount { get; }

        /// <summary>
        /// Returns all direct sub-tables referenced by this table.
        /// </summary>
        public abstract IEnumerable<LootTableAssetBase> GetSubTables();

        /// <summary>
        /// Checks for circular dependencies in the table hierarchy starting from this asset.
        /// </summary>
        /// <param name="stack">A list used to track the current path; if a cycle is found, it will contain the cycle path.</param>
        /// <returns>True if a circular dependency is detected.</returns>
        public bool HasCircularDependency(List<LootTableAssetBase> stack) {
            if (stack == null) return false;

            // Using ReferenceEquals to be absolutely sure we detect the same instance
            foreach (var s in stack) {
                if (ReferenceEquals(s, this)) {
                    stack.Add(this);
                    return true;
                }
            }

            stack.Add(this);
            foreach (var sub in GetSubTables()) {
                if (sub != null && sub.HasCircularDependency(stack)) {
                    return true;
                }
            }
            stack.RemoveAt(stack.Count - 1);
            return false;
        }
    }
}