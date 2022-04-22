using System;
using JetBrains.Annotations;
using Xunit;

namespace AbbLab.SemanticVersioning.Tests
{
    internal static class AssertEx
    {
        private static void CatchException([InstantHandle] Action action, out Exception? exception)
        {
            try
            {
                action();
                exception = null;
            }
            catch (Exception? caught)
            {
                exception = caught;
            }
        }
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

        public static void Identical<T>(Func<T?>[] actions, Action<T?> assert)
        {
            Exception? exception = null;
            CatchException(() => assert(CatchException(actions[0], out exception)), out Exception? assertException);

            for (int i = 1, length = actions.Length; i < length; i++)
            {
                Exception? subsequent = null;
                CatchException(() => assert(CatchException(actions[i], out subsequent)), out Exception? subsequentAssertException);

                Assert.Equal(exception?.GetType(), subsequent?.GetType());
                Assert.Equal(exception?.Message, subsequent?.Message);
                Assert.Equal(assertException?.GetType(), subsequentAssertException?.GetType());
                Assert.Equal(assertException?.Message, subsequentAssertException?.Message);
            }
        }
        public static void Identical(Action[] actions, Action thrower)
        {
            CatchException(thrower, out Exception? exception);

            for (int i = 1, length = actions.Length; i < length; i++)
            {
                CatchException(actions[i], out Exception? subsequent);

                Assert.Equal(exception?.GetType(), subsequent?.GetType());
                Assert.Equal(exception?.Message, subsequent?.Message);
            }
        }

        public delegate bool TryParse<T>(out T? result);
        public static void Identical<T>(TryParse<T>[] actions, Action<T?> assert)
            => Identical(Array.ConvertAll(actions, static a => (Func<T?>)(() => a(out T? res) ? res : default)), assert);

    }
}
