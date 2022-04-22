using System;
using System.Buffers;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    internal static class Utility
    {
        [Pure] public static bool IsValidCharacter(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-';
        [Pure] public static bool IsLetter(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        [Pure] public static bool IsDigit(char c) => c is >= '0' and <= '9';
        [Pure] public static bool IsNumeric(ReadOnlySpan<char> text)
        {
            for (int i = 0, length = text.Length; i < length; i++)
                if (text[i] is < '0' or > '9')
                    return false;
            return true;
        }

        [Pure] public static bool IsValidIdentifier(string str)
        {
            for (int i = 0, length = str.Length; i < length; i++)
                if (!IsValidCharacter(str[i]))
                    return false;
            return true;
        }
        [Pure] public static bool IsValidIdentifier(ReadOnlySpan<char> str)
        {
            for (int i = 0, length = str.Length; i < length; i++)
                if (!IsValidCharacter(str[i]))
                    return false;
            return true;
        }

        [Pure] public static bool TryTrim(string text, out ReadOnlySpan<char> trimmed)
        {
            trimmed = text.AsSpan().Trim();
            return trimmed.Length != text.Length;
        }

        [Pure] public static int SimpleParse(ReadOnlySpan<char> text)
        {
            int result = text[0] - '0';
            for (int i = 1, length = text.Length; i < length; i++)
            {
                result = result * 10 + (text[i] - '0');
                if (result < 0) return -1; // TODO: this check is not reliable at all
            }
            return result;
        }

        [Pure] private static int CountDigits(int number) => number switch
        {
            < 10 => 1,
            < 100 => 2,
            < 1000 => 3,
            < 10000 => 4,
            < 100000 => 5,
            < 1000000 => 6,
            < 10000000 => 7,
            < 100000000 => 8,
            < 1000000000 => 9,
            _ => 10,
        };
        private static void FillNumber(Span<char> buffer, int number)
        {
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                int div = number / 10;
                buffer[i] = (char)('0' + (number - div * 10));
                number = div;
            }
        }
        private static readonly SpanAction<char, int> FillNumberDelegate = FillNumber;

        // TODO: benchmark against the method with stackalloc[16]
        [Pure] public static string SimpleToString(int number)
            => string.Create(CountDigits(number), number, FillNumberDelegate);

    }
}
