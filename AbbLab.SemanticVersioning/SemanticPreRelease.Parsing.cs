using System;
using AbbLab.Extensions;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public readonly partial struct SemanticPreRelease
    {
        // ReSharper disable once UnusedParameter.Local
        private SemanticPreRelease(string identifier, bool _)
        {
            text = identifier;
            number = default;
        }

        [Pure] internal static SemanticErrorCode TryParseSpan(ref ReadOnlySpan<char> span, bool allowLeadingZeroes,
                                                              out SemanticPreRelease preRelease)
        {
            int pos = 0;
            int length = span.Length;
            while (pos < length && Utility.IsValidCharacter(span[pos]))
                pos++;
            if (pos is 0) return Util.Fail(SemanticErrorCode.PreReleaseNotFound, out preRelease);

            ReadOnlySpan<char> text = span[..pos];
            span = span[pos..];
            if (Utility.IsNumeric(text))
            {
                if (!allowLeadingZeroes && text[0] is '0' && text.Length > 1)
                    return Util.Fail(SemanticErrorCode.PreReleaseLeadingZeroes, out preRelease);
                int result = Utility.SimpleParse(text);
                if (result is -1) return Util.Fail(SemanticErrorCode.PreReleaseTooBig, out preRelease);
                preRelease = new SemanticPreRelease(result);
                return SemanticErrorCode.Success;
            }
            preRelease = new SemanticPreRelease(new string(text), false);
            return SemanticErrorCode.Success;
        }

        [Pure] private static SemanticErrorCode ParseInternal(ReadOnlySpan<char> text, bool allowLeadingZeroes, out int result)
        {
            if (text.Length is 0) return Util.Fail(SemanticErrorCode.PreReleaseEmpty, out result);
            if (Utility.IsNumeric(text))
            {
                if (!allowLeadingZeroes && text[0] is '0' && text.Length > 1)
                    return Util.Fail(SemanticErrorCode.PreReleaseLeadingZeroes, out result);
                result = Utility.SimpleParse(text);
                if (result is -1) return SemanticErrorCode.PreReleaseTooBig;
                return SemanticErrorCode.Success;
            }
            if (!Utility.IsValidIdentifier(text)) return Util.Fail(SemanticErrorCode.PreReleaseInvalid, out result);
            result = -1;
            return SemanticErrorCode.Success;
        }
        internal static SemanticErrorCode ParseValidated(ReadOnlySpan<char> text, bool allowLeadingZeroes, out SemanticPreRelease preRelease)
        {
            if (Utility.IsNumeric(text))
            {
                if (!allowLeadingZeroes && text[0] is '0' && text.Length > 1)
                    return Util.Fail(SemanticErrorCode.PreReleaseLeadingZeroes, out preRelease);
                int result = Utility.SimpleParse(text);
                if (result is -1) return Util.Fail(SemanticErrorCode.PreReleaseTooBig, out preRelease);
                preRelease = new SemanticPreRelease(result);
                return SemanticErrorCode.Success;
            }
            preRelease = new SemanticPreRelease(new string(text), false);
            return SemanticErrorCode.Success;
        }

        [Pure] public static SemanticPreRelease Parse(string text)
        {
            SemanticErrorCode code = ParseInternal(text.AsSpan(), false, out int result);
            if (code is not SemanticErrorCode.Success) throw new ArgumentException(code.GetMessage(), nameof(text));
            return result is -1 ? new SemanticPreRelease(text, false) : new SemanticPreRelease(result);
        }
        [Pure] public static SemanticPreRelease Parse(ReadOnlySpan<char> text)
        {
            SemanticErrorCode code = ParseInternal(text, false, out int result);
            if (code is not SemanticErrorCode.Success) throw new ArgumentException(code.GetMessage(), nameof(text));
            return result is -1 ? new SemanticPreRelease(new string(text), false) : new SemanticPreRelease(result);
        }

        [Pure] public static SemanticPreRelease Parse(string text, SemanticOptions options)
        {
            bool allowLeadingZeroes = (options & SemanticOptions.AllowLeadingZeroes) is not 0;

            if ((options & SemanticOptions.AllowInnerWhite) is not 0 && Utility.TryTrim(text, out ReadOnlySpan<char> trimmed))
            {
                SemanticErrorCode code = ParseInternal(trimmed, allowLeadingZeroes, out int result);
                if (code is not SemanticErrorCode.Success) throw new ArgumentException(code.GetMessage(), nameof(text));
                return result is -1 ? new SemanticPreRelease(new string(trimmed), false) : new SemanticPreRelease(result);
            }
            else
            {
                SemanticErrorCode code = ParseInternal(text.AsSpan(), allowLeadingZeroes, out int result);
                if (code is not SemanticErrorCode.Success) throw new ArgumentException(code.GetMessage(), nameof(text));
                return result is -1 ? new SemanticPreRelease(text, false) : new SemanticPreRelease(result);
            }
        }
        [Pure] public static SemanticPreRelease Parse(ReadOnlySpan<char> text, SemanticOptions options)
        {
            if ((options & SemanticOptions.AllowInnerWhite) is not 0)
                text = text.Trim();
            SemanticErrorCode code = ParseInternal(text, (options & SemanticOptions.AllowLeadingZeroes) is not 0, out int result);
            if (code is not SemanticErrorCode.Success) throw new ArgumentException(code.GetMessage(), nameof(text));
            return result is -1 ? new SemanticPreRelease(new string(text), false) : new SemanticPreRelease(result);
        }

    }
}
