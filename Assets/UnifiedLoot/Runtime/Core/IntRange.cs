using System;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Random;
using UnityEngine;

namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Core {
    /// <summary>
    /// Represents an inclusive integer range, typically used for quantity rolls.
    /// </summary>
    [Serializable]
    public struct IntRange : IEquatable<IntRange> {
        public const string NameOfMin = nameof(min);
        public const string NameOfMax = nameof(max);

        [SerializeField] private int min;
        [SerializeField] private int max;

        public int Min => min;
        public int Max => max;

        public IntRange(int value) : this(value, value) { }

        public IntRange(int min, int max) {
            if (min > max)
                throw new ArgumentException($"Min ({min}) cannot be greater than Max ({max}).");

            this.min = min;
            this.max = max;
        }


        /// <summary>
        /// Rolls a random value within the range (inclusive).
        /// </summary>
        public int Roll(IRandom random) => random.Range(min, max + 1);

        public bool Contains(int value) => value >= min && value <= max;
        public bool Equals(IntRange other) => min == other.min && max == other.max;
        public override bool Equals(object? obj) => obj is IntRange other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(min, max);
        public override string ToString() => min == max ? min.ToString() : $"{min}-{max}";

        public static bool operator ==(IntRange left, IntRange right) => left.Equals(right);
        public static bool operator !=(IntRange left, IntRange right) => !left.Equals(right);

        public static IntRange operator *(IntRange left, IntRange right) => new(left.min * right.min, left.max * right.max);
        public static IntRange operator *(IntRange left, int multiplier) => new(left.min * multiplier, left.max * multiplier);

        public static implicit operator IntRange(int value) => new(value);
    }
}