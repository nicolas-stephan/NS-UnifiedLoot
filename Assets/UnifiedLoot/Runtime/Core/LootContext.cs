using System;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// A flexible context container that passes data to loot strategies.
    /// Uses integer-keyed storage for fast lookups.
    /// </summary>
    public class LootContext {
        private readonly Dictionary<int, object> _values = new();

        /// <summary>
        /// Sets a value in the context. Returns this context for chaining.
        /// </summary>
        public LootContext Set<T>(ContextKey<T> key, T value) {
            _values[key.Id] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Gets a value from the context.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found.</exception>
        public T Get<T>(ContextKey<T> key) {
            if (_values.TryGetValue(key.Id, out var value))
                return (T)value;
            throw new KeyNotFoundException($"Context key '{key.Name ?? key.Id.ToString()}' not found.");
        }

        /// <summary>
        /// Tries to get a value from the context.
        /// </summary>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool TryGet<T>(ContextKey<T> key, out T? value) {
            if (_values.TryGetValue(key.Id, out var obj)) {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets a value from the context, or returns the default value if not found.
        /// </summary>
        public T? GetOrDefault<T>(ContextKey<T> key, T? defaultValue = default) => TryGet(key, out var value) ? value : defaultValue;

        /// <summary>
        /// Checks if the context contains a key.
        /// </summary>
        public bool Contains<T>(ContextKey<T> key) => _values.ContainsKey(key.Id);

        /// <summary>
        /// Removes a key from the context.
        /// </summary>
        /// <returns>True if the key was removed, false if it wasn't present.</returns>
        public bool Remove<T>(ContextKey<T> key) => _values.Remove(key.Id);

        /// <summary>
        /// Clears all values from the context.
        /// </summary>
        public void Clear() => _values.Clear();
    }
}