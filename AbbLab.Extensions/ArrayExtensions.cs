using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class ArrayExtensions
    {
        [Pure] public static int IndexOf<T>(this T[] array, T value)
            => Array.IndexOf(array, value);
        [Pure] public static int IndexOf<T>(this T[] array, T value, int startIndex)
            => Array.IndexOf(array, value, startIndex);
        [Pure] public static int IndexOf<T>(this T[] array, T value, int startIndex, int count)
            => Array.IndexOf(array, value, startIndex, count);

        [Pure] public static int LastIndexOf<T>(this T[] array, T value)
            => Array.LastIndexOf(array, value);
        [Pure] public static int LastIndexOf<T>(this T[] array, T value, int startIndex)
            => Array.LastIndexOf(array, value, startIndex);
        [Pure] public static int LastIndexOf<T>(this T[] array, T value, int startIndex, int count)
            => Array.LastIndexOf(array, value, startIndex, count);

        [Pure] public static bool Contains<T>(this T[] array, T value)
            => Array.IndexOf(array, value) is not -1;

        [Pure] public static bool Exists<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.Exists(array, predicate);
        [Pure] public static bool Exists<T>(this T[] array, [InstantHandle] Func<T, int, bool> predicate)
        {
            for (int i = 0, length = array.Length; i < length; i++)
                if (predicate(array[i], i))
                    return true;
            return false;
        }

        [Pure] public static bool TrueForAll<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.TrueForAll(array, predicate);
        [Pure] public static bool TrueForAll<T>(this T[] array, [InstantHandle] Func<T, int, bool> predicate)
        {
            for (int i = 0, length = array.Length; i < length; i++)
                if (!predicate(array[i], i))
                    return false;
            return true;
        }

        [Pure] public static T? Find<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.Find(array, predicate);
        [Pure] public static T? Find<T>(this T[] array, [InstantHandle] Func<T, int, bool> predicate)
        {
            for (int i = 0, length = array.Length; i < length; i++)
                if (predicate(array[i], i))
                    return array[i];
            return default;
        }

        [Pure] public static T? FindLast<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.FindLast(array, predicate);
        [Pure] public static T? FindLast<T>(this T[] array, [InstantHandle] Func<T, int, bool> predicate)
        {
            for (int i = array.Length - 1; i >= 0; i--)
                if (predicate(array[i], i))
                    return array[i];
            return default;
        }

        [Pure] public static int FindIndex<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.FindIndex(array, 0, array.Length, predicate);
        [Pure] public static int FindIndex<T>(this T[] array, int startIndex, [InstantHandle] Predicate<T> predicate)
            => Array.FindIndex(array, startIndex, array.Length - startIndex, predicate);
        [Pure] public static int FindIndex<T>(this T[] array, int startIndex, int count, [InstantHandle] Predicate<T> predicate)
            => Array.FindIndex(array, startIndex, count, predicate);

        [Pure] public static int FindLastIndex<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.FindLastIndex(array, 0, array.Length, predicate);
        [Pure] public static int FindLastIndex<T>(this T[] array, int startIndex, [InstantHandle] Predicate<T> predicate)
            => Array.FindLastIndex(array, startIndex, array.Length - startIndex, predicate);
        [Pure] public static int FindLastIndex<T>(this T[] array, int startIndex, int count, [InstantHandle] Predicate<T> predicate)
            => Array.FindLastIndex(array, startIndex, count, predicate);

        public static void ForEach<T>(this T[] array, [InstantHandle] Action<T> action)
            => Array.ForEach(array, action);
        public static void ForEach<T>(this T[] array, [InstantHandle] Action<T, int> action)
        {
            for (int i = 0, length = array.Length; i < length; i++)
                action(array[i], i);
        }

        [Pure] public static T[] FindAll<T>(this T[] array, [InstantHandle] Predicate<T> predicate)
            => Array.FindAll(array, predicate);
        [Pure] public static T[] FindAll<T>(this T[] array, [InstantHandle] Func<T, int, bool> predicate)
        {
            List<T> list = new List<T>();
            for (int i = 0, length = array.Length; i < length; i++)
                if (predicate(array[i], i))
                    list.Add(array[i]);
            return list.ToArray();
        }

        [Pure] public static TResult[] ConvertAll<T, TResult>(this T[] array, [InstantHandle] Converter<T, TResult> converter)
            => Array.ConvertAll(array, converter);
        [Pure] public static TResult[] ConvertAll<T, TResult>(this T[] array, [InstantHandle] Func<T, int, TResult> converter)
        {
            int length = array.Length;
            TResult[] results = new TResult[length];
            for (int i = 0; i < length; i++)
                results[i] = converter(array[i], i);
            return results;
        }

        public static void Clear<T>(this T[] array)
            => Array.Clear(array, 0, array.Length);
        public static void Clear<T>(this T[] array, int startIndex, int count)
            => Array.Clear(array, startIndex, count);

        public static void Fill<T>(this T[] array, T value)
            => Array.Fill(array, value);
        public static void Fill<T>(this T[] array, T value, int startIndex, int count)
            => Array.Fill(array, value, startIndex, count);

        public static void Reverse<T>(this T[] array)
            => Array.Reverse(array);
        public static void Reverse<T>(this T[] array, int startIndex, int count)
            => Array.Reverse(array, startIndex, count);

        public static void Sort<T>(this T[] array)
            => Array.Sort(array);
        public static void Sort<T>(this T[] array, IComparer<T>? comparer)
            => Array.Sort(array, comparer);
        public static void Sort<T>(this T[] array, [InstantHandle] Comparison<T> comparison)
            => Array.Sort(array, Comparer<T>.Create(comparison));
        public static void Sort<T>(this T[] array, int startIndex, int count)
            => Array.Sort(array, startIndex, count);
        public static void Sort<T>(this T[] array, int startIndex, int count, IComparer<T>? comparer)
            => Array.Sort(array, startIndex, count, comparer);
        public static void Sort<T>(this T[] array, int startIndex, int count, [InstantHandle] Comparison<T> comparison)
            => Array.Sort(array, startIndex, count, Comparer<T>.Create(comparison));

        [Pure] public static TResult[] Cast<TResult>(this Array array)
        {
            int length = array.Length;
            TResult[] result = new TResult[length];
            for (int i = 0; i < length; i++)
                result[i] = (TResult)array.GetValue(i)!;
            return result;
        }
        [Pure] public static TResult[] OfType<TResult>(this Array array)
        {
            List<TResult> list = new List<TResult>();
            for (int i = 0, length = array.Length; i < length; i++)
                if (array.GetValue(i) is TResult result)
                    list.Add(result);
            return list.ToArray();
        }

        public static void Shuffle<T>(this T[] array)
            => Shuffle(array, new Random());
        public static void Shuffle<T>(this T[] array, Random rnd)
        {
            int i = array.Length;
            while (i > 1)
            {
                int index = rnd.Next(i--);
                T value = array[index];
                array[index] = array[i];
                array[i] = value;
            }
        }

    }
}
