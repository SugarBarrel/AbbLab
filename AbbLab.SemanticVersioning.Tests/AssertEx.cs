using System;
using JetBrains.Annotations;
using Xunit;

namespace AbbLab.SemanticVersioning.Tests
{
    internal static class AssertEx
    {
        private static T? CatchException<T>([InstantHandle] Func<T?> action, out Exception? exception)
        {
            try
            {
                T? res = action();
                exception = null;
                return res;
            }
            catch (Exception? caught)
            {
                exception = caught;
                return default;
            }
        }

        public static void Identical<T>(Func<T>[] functions, Action<T?, Exception?> assert)
        {
            for (int i = 0, length = functions.Length; i < length; i++)
            {
                T? value = CatchException(functions[i], out Exception? exception);
                assert(value, exception);
            }
        }
        public static void Identical<T>(TryParse<T>[] tryFunctions, Action<T?, Exception?> assert)
        {
            for (int i = 0, length = tryFunctions.Length; i < length; i++)
            {
                bool res = tryFunctions[i](out T? value);
                Assert.Equal(res, value is not null);
                if (res) assert(value, null);
            }
        }

        public delegate bool TryParse<T>(out T? value);

    }
}
