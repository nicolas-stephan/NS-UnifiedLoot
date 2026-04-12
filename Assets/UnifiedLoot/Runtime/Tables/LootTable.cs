using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// A simple code-defined loot table.
    /// </summary>
    public class LootTable<T> : ILootTable<T> {
        private readonly List<ILootEntry<T>> _entries;

        public int Id { get; }
        public int Count => _entries.Count;
        public ILootEntry<T> this[int index] => _entries[index];

        public LootTable() {
            Id = LootTableIdGenerator.GetNextId();
            _entries = new List<ILootEntry<T>>();
        }

        public LootTable(IEnumerable<ILootEntry<T>> entries) {
            Id = LootTableIdGenerator.GetNextId();
            _entries = new List<ILootEntry<T>>(entries);
        }

        public LootTable<T> AddEmpty(float weight = 1f) {
            _entries.Add(new LootEntry<T>(default, weight));
            return this;
        }

        public LootTable<T> Add(T item, float weight = 1f) {
            _entries.Add(new LootEntry<T>(item, weight));
            return this;
        }

        public LootTable<T> Add(T item, float weight, IntRange quantity) {
            _entries.Add(new LootEntry<T>(item, weight, quantity));
            return this;
        }

        public LootTable<T> Add(T item, float weight, int minQuantity, int maxQuantity) {
            _entries.Add(new LootEntry<T>(item, weight, new IntRange(minQuantity, maxQuantity)));
            return this;
        }

        public LootTable<T> Add(ILootEntry<T> entry) {
            _entries.Add(entry);
            return this;
        }
    }
}
