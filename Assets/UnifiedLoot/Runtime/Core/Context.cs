using System;
using System.Collections.Generic;

namespace NS.UnifiedLoot {
    /// <summary>
    /// A flexible context container.
    /// </summary>
    public class Context {
        private readonly Dictionary<object, object> _values = new();

        /// <summary>
        /// Sets a value in the context. Returns this context for chaining.
        /// </summary>
        public Context Set<T>(Key<T> key, T value) {
            _values[key] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Gets a value from the context.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found.</exception>
        public T Get<T>(Key<T> key) {
            if (_values.TryGetValue(key, out var value))
                return (T)value;
            throw new KeyNotFoundException($"Context key '{key}' not found.");
        }

        /// <summary>
        /// Tries to get a value from the context.
        /// </summary>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool TryGet<T>(Key<T> key, out T? value) {
            if (_values.TryGetValue(key, out var obj)) {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets a value from the context, or returns the default value if not found.
        /// </summary>
        public T? GetOrDefault<T>(Key<T> key, T? defaultValue = default) => TryGet(key, out var value) ? value : defaultValue;

        /// <summary>
        /// Checks if the context contains a key.
        /// </summary>
        public bool Contains<T>(Key<T> key) => _values.ContainsKey(key);

        /// <summary>
        /// Removes a key from the context.
        /// </summary>
        /// <returns>True if the key was removed, false if it wasn't present.</returns>
        public bool Remove<T>(Key<T> key) => _values.Remove(key);

        /// <summary>
        /// Clears all values from the context.
        /// </summary>
        public void Clear() => _values.Clear();
    }
}
