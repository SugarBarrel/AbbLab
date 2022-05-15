using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public sealed class ReverseComparer<T> : IComparer, IComparer<T?>
    {
        public IComparer<T?> Comparer { get; }

        public ReverseComparer(IComparer<T?> comparer) => Comparer = comparer;

        [Pure] public int Compare(T? a, T? b)
        {
            if (b is null) return a is null ? 0 : -1;
            if (a is null) return 1;
            return -Comparer.Compare(a, b);
        }
        [Pure] int IComparer.Compare(object? a, object? b)
        {
            if (b is null) return a is null ? 0 : -1;
            if (a is null) return 1;

            if (a is T tA && b is T tB) return Compare(tA, tB);
            if (a is IComparable comparableA)
                return -comparableA.CompareTo(b);
            if (b is IComparable comparableB)
                return comparableB.CompareTo(a);
            throw new ArgumentException("The objects do not implement the IComparable interface.");
        }

        public static ReverseComparer<T?> Default { get; } = new ReverseComparer<T?>(Comparer<T?>.Default);

        [Pure] public static ReverseComparer<T?> Create(Comparison<T?> comparison)
            => new ReverseComparer<T?>(Comparer<T?>.Create(comparison));

    }
}
