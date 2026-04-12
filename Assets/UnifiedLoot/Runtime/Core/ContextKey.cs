namespace NS.UnifiedLoot {
    /// <summary>
    /// A type-safe key for storing and retrieving values from a <see cref="Context"/>.
    /// </summary>
    /// <typeparam name="T">The type of value this key represents.</typeparam>
    public sealed class Key<T> {
        /// <summary>
        /// Optional name for debugging purposes.
        /// </summary>
        public string? Name { get; }

        public Key(string? name = null) => Name = name;
        public override string ToString() => Name != null ? $"Key<{typeof(T).Name}>({Name})" : $"Key<{typeof(T).Name}>()";
    }

    public sealed class Key {
        /// <summary>
        /// Optional name for debugging purposes.
        /// </summary>
        public string? Name { get; }

        public Key(string? name = null) => Name = name;
        public override string ToString() => Name != null ? $"Key({Name})" : "Key()";
    }
}
