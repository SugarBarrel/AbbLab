﻿using System;
using System.Text;
using AbbLab.Extensions;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public readonly struct PartialComponent : IEquatable<PartialComponent>, IComparable, IComparable<PartialComponent>,
                                              IFormattable
    {
        private readonly int _value;

        // ReSharper disable once UnusedParameter.Local
        private PartialComponent(int value, bool _)
            => _value = value;

        public PartialComponent(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, Exceptions.ComponentNegative);
            _value = value;
        }
        public PartialComponent(char character) => this = Parse(character);

        public static readonly PartialComponent Zero = new PartialComponent(0);
        public static readonly PartialComponent LowercaseX = new PartialComponent('x');
        public static readonly PartialComponent UppercaseX = new PartialComponent('X');
        public static readonly PartialComponent Star = new PartialComponent('*');

        public bool IsNumeric => _value >= 0;
        public bool IsWildcard => _value < 0;
        public int Number => _value >= 0 ? _value : throw new InvalidOperationException(Exceptions.ComponentNotNumeric);
        public char Wildcard => _value < 0 ? (char)-_value : throw new InvalidOperationException(Exceptions.ComponentNotWildcard);

        [Pure] public bool Equals(PartialComponent other)
        {
            if (_value < 0) return other._value < 0;
            return _value == other._value;
        }
        [Pure] public override bool Equals(object? obj)
            => obj is PartialComponent other && Equals(other);
        [Pure] public override int GetHashCode() => _value;

        [Pure] public int CompareTo(PartialComponent other)
        {
            if (_value < 0) return other._value < 0 ? 0 : -1;
            if (other._value < 0) return 1;
            return _value - other._value;
        }
        [Pure] int IComparable.CompareTo(object? obj)
        {
            if (obj is PartialComponent other) return CompareTo(other);
            throw new ArgumentException($"Object must be of type {nameof(PartialComponent)}", nameof(obj));
        }

        [Pure] public static bool operator ==(PartialComponent a, PartialComponent b) => a.Equals(b);
        [Pure] public static bool operator !=(PartialComponent a, PartialComponent b) => !a.Equals(b);

        [Pure] public static bool operator >(PartialComponent a, PartialComponent b) => a.CompareTo(b) > 0;
        [Pure] public static bool operator <(PartialComponent a, PartialComponent b) => a.CompareTo(b) < 0;
        [Pure] public static bool operator >=(PartialComponent a, PartialComponent b) => a.CompareTo(b) >= 0;
        [Pure] public static bool operator <=(PartialComponent a, PartialComponent b) => a.CompareTo(b) <= 0;

        [Pure] public override string ToString()
            => _value < 0 ? ((char)-_value).ToString() : Utility.SimpleToString(_value);
        [Pure] public string ToString(ReadOnlySpan<char> format)
        {
            int length = format.Length;
            if (length is 1 && format[0] is 'G' or 'g') return ToString();
            if (length is 0) return string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (format[i] is not 'x' and not 'X' and not '*' and not '0')
                    return ToStringBuilder(format);
            }
            if (IsNumeric) return Utility.SimpleToString(_value);
            return ToStringSingleWildcard(format).ToString();
        }
        [Pure] private char ToStringSingleWildcard(ReadOnlySpan<char> format)
        {
            char myWildcard = (char)-_value;

            int lastIndex = format.Length - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                char c = format[i];
                if (c == myWildcard) return myWildcard;
            }
            return format[lastIndex];
        }
        [Pure] private string ToStringBuilder(ReadOnlySpan<char> format)
        {
            StringBuilder sb = new StringBuilder();

            int pos = 0;
            int length = format.Length;
            char quote = default;
            int quoteStart = 0;

            while (pos < length)
            {
                if (quote != default)
                {
                    if (format[pos] == quote)
                    {
                        sb.Append(format[quoteStart..pos]);
                        quote = default;
                    }
                    pos++;
                    continue;
                }

                char c = format[pos];
                switch (c)
                {
                    case '\\':
                        sb.Append(++pos < length ? format[pos] : '\\');
                        break;
                    case '\'' or '"':
                        quote = c;
                        quoteStart = pos + 1;
                        break;
                    case 'x' or 'X' or '*' or '0':
                        int start = pos++;
                        while (pos < length && format[pos] is 'x' or 'X' or '*' or '0')
                            pos++;
                        if (IsNumeric) sb.SimpleAppend(_value);
                        else sb.Append(ToStringSingleWildcard(format[start..pos]));
                        continue;
                }
                pos++;
            }
            if (quote != default) throw new FormatException("The format string contains an unclosed quote ('\\'', '\"').");

            return sb.ToString();
        }

        [Pure] public string ToString(string? format)
            => format is null ? ToString() : ToString(format.AsSpan());
        [Pure] string IFormattable.ToString(string? format, IFormatProvider? provider)
            => ToString(format);

        [Pure] private static SemanticErrorCode ParseInternal(char character, out PartialComponent component)
        {
            switch (character)
            {
                case 'x' or 'X' or '*':
                    component = new PartialComponent(-character, default);
                    return SemanticErrorCode.Success;
                case >= '0' and <= '9':
                    component = new PartialComponent(character - '0', default);
                    return SemanticErrorCode.Success;
                default:
                    component = default;
                    return SemanticErrorCode.ComponentInvalid;
            }
        }
        [Pure] private static SemanticErrorCode ParseInternal(ReadOnlySpan<char> text, SemanticOptions options, out PartialComponent component)
        {
            const SemanticOptions trimMask = SemanticOptions.AllowLeadingWhite | SemanticOptions.AllowTrailingWhite;
            switch (options & trimMask)
            {
                case trimMask:
                    text = text.Trim();
                    break;
                case SemanticOptions.AllowLeadingWhite:
                    text = text.TrimStart();
                    break;
                case SemanticOptions.AllowTrailingWhite:
                    text = text.TrimEnd();
                    break;
            }
            if (text.Length is 1) return ParseInternal(text[0], out component);

            if (Utility.IsNumeric(text))
            {
                if ((options & SemanticOptions.AllowLeadingZeroes) is 0 && text.Length > 1 && text[0] is '0')
                    return Util.Fail(SemanticErrorCode.ComponentLeadingZeroes, out component);

                int result = Utility.SimpleParse(text);
                if (result is -1) return Util.Fail(SemanticErrorCode.ComponentTooBig, out component);
                component = new PartialComponent(result, default);
                return SemanticErrorCode.Success;
            }
            return Util.Fail(SemanticErrorCode.ComponentInvalid, out component);
        }

        [Pure] public static PartialComponent Parse(char character)
        {
            SemanticErrorCode code = ParseInternal(character, out PartialComponent component);
            if (code is SemanticErrorCode.Success) return component;
            throw new ArgumentException(code.GetMessage(), nameof(character));
        }
        [Pure] public static bool TryParse(char character, out PartialComponent component)
            => ParseInternal(character, out component) is SemanticErrorCode.Success;

        [Pure] public static PartialComponent Parse(ReadOnlySpan<char> text, SemanticOptions options)
        {
            SemanticErrorCode code = ParseInternal(text, options, out PartialComponent component);
            if (code is SemanticErrorCode.Success) return component;
            throw new ArgumentException(code.GetMessage(), nameof(text));
        }
        [Pure] public static PartialComponent Parse(string text, SemanticOptions options)
            => Parse(text.AsSpan(), options);
        [Pure] public static PartialComponent Parse(ReadOnlySpan<char> text)
            => Parse(text, SemanticOptions.Strict);
        [Pure] public static PartialComponent Parse(string text)
            => Parse(text.AsSpan(), SemanticOptions.Strict);

        [Pure] public static bool TryParse(ReadOnlySpan<char> text, SemanticOptions options, out PartialComponent component)
            => ParseInternal(text, options, out component) is SemanticErrorCode.Success;
        [Pure] public static bool TryParse(string text, SemanticOptions options, out PartialComponent component)
            => TryParse(text.AsSpan(), options, out component);
        [Pure] public static bool TryParse(ReadOnlySpan<char> text, out PartialComponent component)
            => TryParse(text, SemanticOptions.Strict, out component);
        [Pure] public static bool TryParse(string text, out PartialComponent component)
            => TryParse(text.AsSpan(), SemanticOptions.Strict, out component);

    }
}
