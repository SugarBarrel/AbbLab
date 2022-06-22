using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the minimum value for.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the minimum value for.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
        [Pure] public static T WithMin<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector)
            => WithMin(source, selector, null);
        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the minimum value for.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> generic interface implementation to use when comparing values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the minimum value for.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
        [Pure] public static T WithMin<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector,
                                                   IComparer<TResult>? comparer)
        {
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new InvalidOperationException($"{nameof(source)} is empty.");
                T minT = enumerator.Current;
                TResult min = selector(minT);

                comparer ??= Comparer<TResult>.Default;
                while (enumerator.MoveNext())
                {
                    T nextT = enumerator.Current;
                    TResult next = selector(nextT);
                    if (comparer.Compare(next, min) < 0)
                    {
                        min = next;
                        minT = nextT;
                    }
                }
                return minT;
            }
        }

        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the maximum value for.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the maximum value for.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
        [Pure] public static T WithMax<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector)
            => WithMax(source, selector, null);
        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the maximum value for.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> generic interface implementation to use when comparing values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the maximum value for.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
        [Pure] public static T WithMax<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector,
                                                   IComparer<TResult>? comparer)
        {
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new InvalidOperationException($"{nameof(source)} is empty.");
                T maxT = enumerator.Current;
                TResult max = selector(maxT);

                comparer ??= Comparer<TResult>.Default;
                while (enumerator.MoveNext())
                {
                    T nextT = enumerator.Current;
                    TResult next = selector(nextT);
                    if (comparer.Compare(next, max) > 0)
                    {
                        max = next;
                        maxT = nextT;
                    }
                }
                return maxT;
            }
        }

        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the minimum value for, or <see langword="default"/>, if the sequence is empty.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the minimum value for, or <see langword="default"/>, if the sequence is empty.</returns>
        [Pure] public static T? WithMinOrDefault<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector)
            => WithMinOrDefault(source, selector, null);
        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the minimum value for, or <see langword="default"/>, if the sequence is empty.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> generic interface implementation to use when comparing values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the minimum value for, or <see langword="default"/>, if the sequence is empty.</returns>
        [Pure] public static T? WithMinOrDefault<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector,
                                                             IComparer<TResult>? comparer)
        {
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default;
                T minT = enumerator.Current;
                TResult min = selector(minT);

                comparer ??= Comparer<TResult>.Default;
                while (enumerator.MoveNext())
                {
                    T nextT = enumerator.Current;
                    TResult next = selector(nextT);
                    if (comparer.Compare(next, min) < 0)
                    {
                        min = next;
                        minT = nextT;
                    }
                }
                return minT;
            }
        }

        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the maximum value for, or <see langword="default"/>, if the sequence is empty.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the maximum value for, or <see langword="default"/>, if the sequence is empty.</returns>
        [Pure] public static T? WithMaxOrDefault<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector)
            => WithMaxOrDefault(source, selector, null);
        /// <summary>
        ///   <para>Returns an object from the sequence that the specified <paramref name="selector"/> returned the maximum value for, or <see langword="default"/>, if the sequence is empty.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the values of the objects.</typeparam>
        /// <param name="source">The sequence of values to search.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the elements' values.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> generic interface implementation to use when comparing values.</param>
        /// <returns>An object from the sequence that the <paramref name="selector"/> returned the maximum value for, or <see langword="default"/>, if the sequence is empty.</returns>
        [Pure] public static T? WithMaxOrDefault<T, TResult>([InstantHandle] this IEnumerable<T> source, [InstantHandle] Func<T, TResult> selector,
                                                             IComparer<TResult>? comparer)
        {
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return default;
                T maxT = enumerator.Current;
                TResult max = selector(maxT);

                comparer ??= Comparer<TResult>.Default;
                while (enumerator.MoveNext())
                {
                    T nextT = enumerator.Current;
                    TResult next = selector(nextT);
                    if (comparer.Compare(next, max) > 0)
                    {
                        max = next;
                        maxT = nextT;
                    }
                }
                return maxT;
            }
        }

        /// <summary>
        ///   <para>Returns elements from a sequence with distinct values returned by the specified <paramref name="selector"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the objects' unique values.</typeparam>
        /// <param name="source">The sequence to remove elements with duplicate values from.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the unique identifier of an element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains elements with distinct values from the source sequence.</returns>
        [Pure, LinqTunnel] public static IEnumerable<T> WithDistinct<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
            => WithDistinct(source, selector, null);
        /// <summary>
        ///   <para>Returns elements from a sequence with distinct values returned by the specified <paramref name="selector"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the objects' unique values.</typeparam>
        /// <param name="source">The sequence to remove elements with duplicate values from.</param>
        /// <param name="selector">The <see cref="Func{T, TResult}"/> to use to determine the unique identifier of an element.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> generic interface implementation to use when comparing values.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains elements with distinct values from the source sequence.</returns>
        [Pure, LinqTunnel] public static IEnumerable<T> WithDistinct<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector,
                                                                                 IEqualityComparer<TResult>? comparer)
        {
            HashSet<TResult> set = new HashSet<TResult>(comparer ?? EqualityComparer<TResult>.Default);
            foreach (T item in source)
            {
                TResult result = selector(item);
                if (set.Add(result)) yield return item;
            }
        }

        /// <summary>
        ///   <para>Returns non-null elements from a sequence.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to enumerate.</typeparam>
        /// <param name="source">The sequence to remove null elements from.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains non-null elements from the source sequence.</returns>
        [Pure, LinqTunnel] public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
            => (default(T) != null ? source : source.Where(static el => el != null))!;

    }
}
