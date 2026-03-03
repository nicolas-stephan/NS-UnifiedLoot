using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NS.UnifiedLoot {
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
    public abstract class LootTableAsset<TItem> : LootTableAssetBase, ILootTable<TItem> {
        public const string NameOfEntries = nameof(entries);

        [SerializeField] private List<LootEntryData> entries = new();

        private int _id;

        public int Id => _id != 0 ? _id : (_id = LootTableIdGenerator.GetNextId());
        public int Count => entries.Count;

        public override int TableId => Id;
        public override int EntryCount => Count;

        public IEnumerator<ILootEntry<TItem>> GetEnumerator() {
            foreach (var entry in entries)
                yield return entry;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#if UNITY_EDITOR
        private void OnValidate() {
            switch (entries.Count) {
                case 0:
                    Debug.LogWarning($"[UnifiedLoot] Table '{name}' has no entries.", this);
                    return;
                case 1:
                    Debug.LogWarning($"[UnifiedLoot] Table '{name}' has only one entry — weighted selection is degenerate.", this);
                    break;
            }

            for (var i = 0; i < entries.Count; i++) {
                var entry = entries[i];
                if (entry.Weight <= 0f)
                    Debug.LogWarning($"[UnifiedLoot] Table '{name}' entry #{i} has weight <= 0.", this);
                if (entry.Item is null)
                    Debug.LogWarning($"[UnifiedLoot] Table '{name}' entry #{i} has a null item reference.", this);
            }
        }
#endif

        /// <summary>
        /// Entry data stored in the asset.
        /// </summary>
        [Serializable]
        public class LootEntryData : LootEntryDataBase, ILootEntry<TItem> {
            public const string NameOfItem = nameof(item);
            [SerializeField] private TItem? item;

            public TItem? Item => item;
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
        public IntRange Quantity => quantity;
    }

    /// <summary>
    /// Non-generic base for editor tooling. Not intended for direct use.
    /// </summary>
    public abstract class LootTableAssetBase : ScriptableObject {
        public abstract int TableId { get; }
        public abstract int EntryCount { get; }
    }
}