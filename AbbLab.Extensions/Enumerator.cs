using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class Enumerator
    {
        [Pure] public static IEnumerator Empty() => EmptyInstance;
        [Pure] public static IEnumerator<T> Empty<T>() => Typed<T>.EmptyInstance;

        private static readonly IEnumerator EmptyInstance = new EmptyEnumerator();
        private sealed class EmptyEnumerator : IEnumerator
        {
            public object? Current => null;
            public bool MoveNext() => false;
            public void Reset() { }
        }

        private static class Typed<T>
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly IEnumerator<T> EmptyInstance = new EmptyEnumerator<T>();
        }
        private sealed class EmptyEnumerator<T> : IEnumerator<T>
        {
            public T Current => default!;
            object? IEnumerator.Current => default(T);
            public bool MoveNext() => false;
            public void Reset() { }
            public void Dispose() { }
        }

    }
}
