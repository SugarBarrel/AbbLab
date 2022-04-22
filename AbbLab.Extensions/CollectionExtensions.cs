using System;
using System.Collections.Generic;

namespace AbbLab.Extensions
{
    public static class CollectionExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
            => Shuffle(list, new Random());
        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            int i = list.Count;
            while (i > 1)
            {
                int index = rnd.Next(i--);
                T value = list[index];
                list[index] = list[i];
                list[i] = value;
            }
        }
    }
}
