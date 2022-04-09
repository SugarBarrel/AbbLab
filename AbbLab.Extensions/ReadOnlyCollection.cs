using System;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class ReadOnlyCollection
    {
        [Pure] public static ReadOnlyCollection<T> Empty<T>() => Typed<T>.Empty;

        private static class Typed<T>
        {
            public static readonly ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(Array.Empty<T>());
        }
    }
}
