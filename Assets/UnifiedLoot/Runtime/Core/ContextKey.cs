using System;
using System.Threading;

namespace NS.UnifiedLoot {
    
    
    internal readonly struct ContextKey {
        private static int _nextId;
        internal static int GetNext() => Interlocked.Increment(ref _nextId);
    }
    
    /// <summary>
    /// A type-safe key for storing and retrieving values from a <see cref="LootContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of value this key represents.</typeparam>
    public readonly struct ContextKey<T> : IEquatable<ContextKey<T>> {
        public int Id { get; }

        /// <summary>
        /// Optional name for debugging purposes.
        /// </summary>
        public string? Name { get; }

        public ContextKey(string? name = null) {
            Id = ContextKey.GetNext();
            Name = name;
        }

        public bool Equals(ContextKey<T> other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is ContextKey<T> other && Equals(other);
        public override int GetHashCode() => Id;
        public override string ToString() => Name != null ? $"ContextKey<{typeof(T).Name}>({Name})" : $"ContextKey<{typeof(T).Name}>({Id})";

        public static bool operator ==(ContextKey<T> left, ContextKey<T> right) => left.Id == right.Id;
        public static bool operator !=(ContextKey<T> left, ContextKey<T> right) => left.Id != right.Id;
    }
}