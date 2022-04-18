using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class Util
    {
        [Pure] public static bool Fail<T>(out T? result)
        {
            result = default;
            return false;
        }
        [Pure] public static bool Fail<T1, T2>(out T1? result1, out T2? result2)
        {
            result1 = default;
            result2 = default;
            return false;
        }
        [Pure] public static bool Fail<T1, T2, T3>(out T1? result1, out T2? result2, out T3? result3)
        {
            result1 = default;
            result2 = default;
            result3 = default;
            return false;
        }

        [Pure] [return: NotNullIfNotNull("returnValue")]
        public static TReturn? Fail<TReturn, T>(TReturn? returnValue, out T? result)
        {
            result = default;
            return returnValue;
        }
        [Pure] [return: NotNullIfNotNull("returnValue")]
        public static TReturn? Fail<TReturn, T1, T2>(TReturn? returnValue, out T1? result1, out T2? result2)
        {
            result1 = default;
            result2 = default;
            return returnValue;
        }
        [Pure] [return: NotNullIfNotNull("returnValue")]
        public static TReturn? Fail<TReturn, T1, T2, T3>(TReturn? returnValue, out T1? result1, out T2? result2, out T3? result3)
        {
            result1 = default;
            result2 = default;
            result3 = default;
            return returnValue;
        }

    }
}
