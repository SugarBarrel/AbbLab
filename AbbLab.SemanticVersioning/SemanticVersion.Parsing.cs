using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public sealed partial class SemanticVersion
    {
        [Pure] private static SemanticErrorCode ParseInternal(ReadOnlySpan<char> text, out SemanticVersion? version)
        {
            version = null;
            int pos = 0;
            int length = text.Length;

            while (pos < length && text[pos] is >= '0' and <= '9') pos++;
            if (pos is 0) return SemanticErrorCode.MajorNotFound;
            if (text[0] is '0' && pos > 1) return SemanticErrorCode.MajorLeadingZeroes;
            int major = Utility.SimpleParse(text[..pos]);
            if (major is -1) return SemanticErrorCode.MajorTooBig;
            // read major

            if (pos >= length || text[pos] is not '.') return SemanticErrorCode.MinorNotFound;
            pos++; // skip '.'

            int start = pos; // set minor's beginning
            while (pos < length && text[pos] is >= '0' and <= '9') pos++;
            if (pos == start) return SemanticErrorCode.MinorNotFound;
            if (text[start] is '0' && pos > start + 1) return SemanticErrorCode.MinorLeadingZeroes;
            int minor = Utility.SimpleParse(text[start..pos]);
            if (minor is -1) return SemanticErrorCode.MinorTooBig;
            // read minor

            if (pos >= length || text[pos] is not '.') return SemanticErrorCode.PatchNotFound;
            pos++; // skip '.'

            start = pos; // set patch's beginning
            while (pos < length && text[pos] is >= '0' and <= '9') pos++;
            if (pos == start) return SemanticErrorCode.PatchNotFound;
            if (text[start] is '0' && pos > start + 1) return SemanticErrorCode.PatchLeadingZeroes;
            int patch = Utility.SimpleParse(text[start..pos]);
            if (patch is -1) return SemanticErrorCode.PatchTooBig;
            // read patch

            SemanticPreRelease[]? preReleases = null;
            if (pos < length && text[pos] is '-')
            {
                List<SemanticPreRelease> list = new List<SemanticPreRelease>();
                do
                {
                    start = ++pos; // skip '-'/'.' and set pre-release's beginning
                    while (pos < length && Utility.IsValidCharacter(text[pos])) pos++;
                    if (pos == start) return SemanticErrorCode.PreReleaseNotFound;

                    SemanticErrorCode code = SemanticPreRelease.ParseValidated(
                        text[start..pos], false, out SemanticPreRelease preRelease);
                    if (code is not SemanticErrorCode.Success) return code;

                    list.Add(preRelease); // add the read pre-release
                }
                while (pos < length && text[pos] is '.');
                preReleases = list.ToArray();
            }

            string[]? buildMetadata = null;
            if (pos < length && text[pos] is '+')
            {
                List<string> list = new List<string>();
                do
                {
                    start = ++pos; // skip '+'/'.' and set the beginning of a build metadata identifier
                    while (pos < length && Utility.IsValidCharacter(text[pos])) pos++;
                    if (pos == start) return SemanticErrorCode.BuildMetadataNotFound;

                    list.Add(new string(text[start..pos])); // add the read build metadata identifier
                }
                while (pos < length && text[pos] is '.');
                buildMetadata = list.ToArray();
            }

            if (pos < length) return SemanticErrorCode.LEFTOVERS;

            version = new SemanticVersion(major, minor, patch, preReleases, buildMetadata);
            return SemanticErrorCode.Success;
        }
        [Pure] private static SemanticErrorCode ParseInternal(ReadOnlySpan<char> text, SemanticOptions options,
                                                              ref int pos, out SemanticVersion? version)
        {
            version = null;
            int length = text.Length;

            bool allowInnerWhite = (options & SemanticOptions.AllowInnerWhite) is not 0;
            bool allowLeadingZeroes = (options & SemanticOptions.AllowLeadingZeroes) is not 0;

            static void SkipWhitespace(ReadOnlySpan<char> text, ref int pos, int length)
            {
                while (pos < length && char.IsWhiteSpace(text[pos])) pos++;
            }

            if ((options & SemanticOptions.AllowLeadingWhite) is not 0)
                SkipWhitespace(text, ref pos, length);

            if ((options & SemanticOptions.AllowEqualsPrefix) is not 0 && pos < length && text[pos] is '=')
            {
                pos++;
                if (allowInnerWhite) SkipWhitespace(text, ref pos, length);
            }
            if ((options & SemanticOptions.AllowVersionPrefix) is not 0 && pos < length && text[pos] is 'v' or 'V')
            {
                pos++;
                if (allowInnerWhite) SkipWhitespace(text, ref pos, length);
            }

            int start = pos; // set major's beginning
            while (pos < length && text[pos] is >= '0' and <= '9') pos++;
            if (pos == start) return SemanticErrorCode.MajorNotFound;
            if (!allowLeadingZeroes && text[start] is '0' && pos > start + 1) return SemanticErrorCode.MajorLeadingZeroes;
            int major = Utility.SimpleParse(text[start..pos]);
            if (major is -1) return SemanticErrorCode.MajorTooBig;
            // read major

            if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

            int minor = 0, patch = 0;
            if (pos >= length || text[pos] is not '.') // couldn't find a '.'
            {
                if ((options & SemanticOptions.OptionalMinor) is 0)
                    return SemanticErrorCode.MinorNotFound;
            }
            else
            {
                pos++; // skip '.'
                if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                start = pos; // set minor's beginning
                while (pos < length && text[pos] is >= '0' and <= '9') pos++;
                if (pos == start) // skipped '.' but couldn't find any digits
                {
                    if ((options & SemanticOptions.OptionalMinor) is 0)
                        return SemanticErrorCode.MinorNotFound;
                }
                else // found minor's digits
                {
                    if (!allowLeadingZeroes && text[start] is '0' && pos > start + 1) return SemanticErrorCode.MinorLeadingZeroes;
                    minor = Utility.SimpleParse(text[start..pos]);
                    if (minor is -1) return SemanticErrorCode.MinorTooBig;
                    // read minor

                    if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                    if (pos >= length || text[pos] is not '.') // couldn't find a '.'
                    {
                        if ((options & SemanticOptions.OptionalPatch) is 0)
                            return SemanticErrorCode.PatchNotFound;
                    }
                    else
                    {
                        pos++; // skip '.'
                        if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                        start = pos; // set patch's beginning
                        while (pos < length && text[pos] is >= '0' and <= '9') pos++;
                        if (pos == start)
                        {
                            if ((options & SemanticOptions.OptionalPatch) is 0)
                                return SemanticErrorCode.PatchNotFound;
                        }
                        else // found patch's digits
                        {
                            if (!allowLeadingZeroes && text[start] is '0' && pos > start + 1) return SemanticErrorCode.PatchLeadingZeroes;
                            patch = Utility.SimpleParse(text[start..pos]);
                            if (patch is -1) return SemanticErrorCode.PatchTooBig;
                            // read patch

                            if (allowInnerWhite) SkipWhitespace(text, ref pos, length);
                        }
                    }
                }
            }

            SemanticPreRelease[]? preReleases = null;
            if (pos < length)
            {
                if (text[pos] is '-')
                {
                    bool removeEmpty = (options & SemanticOptions.RemoveEmptyPreReleases) is not 0;
                    List<SemanticPreRelease> list = new List<SemanticPreRelease>();
                    do
                    {
                        if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                        start = ++pos; // skip '-'/'.' and set pre-release's beginning
                        while (pos < length && Utility.IsValidCharacter(text[pos])) pos++;
                        if (pos == start)
                        {
                            if (removeEmpty) continue;
                            return SemanticErrorCode.PreReleaseNotFound;
                        }

                        SemanticErrorCode code = SemanticPreRelease.ParseValidated(
                            text[start..pos], allowLeadingZeroes, out SemanticPreRelease preRelease);
                        if (code is not SemanticErrorCode.Success) return code;

                        if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                        list.Add(preRelease); // add the read pre-release
                    }
                    while (pos < length && text[pos] is '.');
                    preReleases = list.ToArray();
                }
                else if ((options & SemanticOptions.OptionalPreReleaseSeparator) is not 0 && Utility.IsValidCharacter(text[pos]))
                {
                    if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                    List<SemanticPreRelease> list = new List<SemanticPreRelease>();
                    do
                    {
                        bool isNumeric = Utility.IsDigit(text[pos]);
                        start = pos; // don't skip anything, since it's already on a pre-release character
                        while (pos < length && (isNumeric ? Utility.IsDigit(text[pos]) : Utility.IsLetter(text[pos]))) pos++;
                        // no need to check for empty pre-releases, since there's always at least one character

                        SemanticErrorCode code = SemanticPreRelease.ParseValidated(
                            text[start..pos], allowLeadingZeroes, out SemanticPreRelease preRelease);
                        if (code is not SemanticErrorCode.Success) return code;

                        if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                        list.Add(preRelease); // add the read pre-release
                    }
                    while (pos < length && Utility.IsValidCharacter(text[pos]));
                    preReleases = list.ToArray();
                }
            }

            string[]? buildMetadata = null;
            if (pos < length && text[pos] is '+')
            {
                bool removeEmpty = (options & SemanticOptions.RemoveEmptyBuildMetadata) is not 0;
                List<string> list = new List<string>();
                do
                {
                    if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                    start = ++pos; // skip '+'/'.' and set the beginning of a build metadata identifier
                    while (pos < length && Utility.IsValidCharacter(text[pos])) pos++;
                    if (pos == start)
                    {
                        if (removeEmpty) continue;
                        return SemanticErrorCode.BuildMetadataNotFound;
                    }

                    if (allowInnerWhite) SkipWhitespace(text, ref pos, length);

                    list.Add(new string(text[start..pos])); // add the read build metadata identifier
                }
                while (pos < length && text[pos] is '.');
                buildMetadata = list.ToArray();
            }

            if (allowInnerWhite) // inner whitespace setting already skipped whitespace
            {
                if ((options & SemanticOptions.AllowTrailingWhite) is 0 && char.IsWhiteSpace(text[pos - 1]))
                    return SemanticErrorCode.LEFTOVERS;
            }
            else if ((options & SemanticOptions.AllowTrailingWhite) is not 0)
                SkipWhitespace(text, ref pos, length);

            if (pos < length && (options & SemanticOptions.AllowLeftovers) is 0) return SemanticErrorCode.LEFTOVERS;

            version = new SemanticVersion(major, minor, patch, preReleases, buildMetadata);
            return SemanticErrorCode.Success;
        }

        [Pure] public static SemanticVersion Parse(string text)
            => Parse(text.AsSpan());
        [Pure] public static SemanticVersion Parse(ReadOnlySpan<char> text)
        {
            SemanticErrorCode code = ParseInternal(text, out SemanticVersion? version);
            if (code is SemanticErrorCode.Success) return version!;
            throw new ArgumentException(code.GetMessage(), nameof(text));
        }
        [Pure] public static bool TryParse(string text, [NotNullWhen(true)] out SemanticVersion? version)
            => TryParse(text.AsSpan(), out version);
        [Pure] public static bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out SemanticVersion? version)
            => ParseInternal(text, out version) is SemanticErrorCode.Success;

        [Pure] public static SemanticVersion Parse(string text, SemanticOptions options)
            => Parse(text.AsSpan(), options);
        [Pure] public static SemanticVersion Parse(ReadOnlySpan<char> text, SemanticOptions options)
        {
            if (options is SemanticOptions.Strict) return Parse(text);
            int pos = 0;
            SemanticErrorCode code = ParseInternal(text, options, ref pos, out SemanticVersion? version);
            if (code is SemanticErrorCode.Success) return version!;
            throw new ArgumentException(code.GetMessage(), nameof(text));
        }
        [Pure] public static bool TryParse(string text, SemanticOptions options, [NotNullWhen(true)] out SemanticVersion? version)
            => TryParse(text.AsSpan(), options, out version);
        [Pure] public static bool TryParse(ReadOnlySpan<char> text, SemanticOptions options, [NotNullWhen(true)] out SemanticVersion? version)
        {
            if (options is SemanticOptions.Strict) return TryParse(text, out version);
            int pos = 0;
            return ParseInternal(text, options, ref pos, out version) is SemanticErrorCode.Success;
        }

        [Pure] public static bool TryParse(string text, SemanticOptions options, out int lastPosition,
                                           [NotNullWhen(true)] out SemanticVersion? version)
            => TryParse(text.AsSpan(), options, out lastPosition, out version);
        [Pure] public static bool TryParse(ReadOnlySpan<char> text, SemanticOptions options, out int lastPosition,
                                           [NotNullWhen(true)] out SemanticVersion? version)
        {
            int pos = 0;
            SemanticErrorCode code = ParseInternal(text, options, ref pos, out version);
            lastPosition = pos;
            return code is SemanticErrorCode.Success;
        }

    }
}
