using System;
using System.Text;

namespace AbbLab.SemanticVersioning
{
    public sealed partial class SemanticVersion : IFormattable
    {
        private int CalculateLength()
        {
            int length = Utility.CountDigits(Major) + Utility.CountDigits(Minor) + Utility.CountDigits(Patch) + 2;
            int preReleasesLength = _preReleases.Length;
            if (preReleasesLength > 0)
            {
                length += preReleasesLength; // '-' and '.' in between
                for (int i = 0; i < preReleasesLength; i++)
                {
                    ref SemanticPreRelease preRelease = ref _preReleases[i];
                    length += preRelease.IsNumeric ? Utility.CountDigits(preRelease.Number) : preRelease.Text.Length;
                }
            }
            int buildMetadataLength = _buildMetadata.Length;
            if (buildMetadataLength > 0)
            {
                length += buildMetadataLength; // '+' and '.' in between
                for (int i = 0; i < buildMetadataLength; i++)
                    length += _buildMetadata[i].Length;
            }
            return length;
        }
        public override string ToString()
        {
            int length = CalculateLength();
            StringBuilder sb = new StringBuilder(length)
                               .SimpleAppend(Major).Append('.')
                               .SimpleAppend(Minor).Append('.')
                               .SimpleAppend(Patch);

            int preReleasesLength = _preReleases.Length;
            if (preReleasesLength > 0)
            {
                sb.Append('-').SimpleAppend(_preReleases[0]);
                for (int i = 1; i < preReleasesLength; i++)
                    sb.Append('.').SimpleAppend(_preReleases[i]);
            }
            int buildMetadataLength = _buildMetadata.Length;
            if (buildMetadataLength > 0)
            {
                sb.Append('+').Append(_buildMetadata[0]);
                for (int i = 1; i < buildMetadataLength; i++)
                    sb.Append('.').Append(_buildMetadata[i]);
            }
            return sb.ToString();
        }

        public string ToString(ReadOnlySpan<char> format)
        {
            if (format.Length is 1 && format[0] is 'G' or 'g') return ToString();

            StringBuilder sb = new StringBuilder();

            int pos = 0;
            int length = format.Length;

            char separator = default;
            static void FlushSeparator(StringBuilder sb, ref char separator)
            {
                if (separator == default) return;
                sb.Append(separator);
                separator = default;
            }
            char quote = default;
            int quoteStart = 0;

            while (pos < length)
            {
                if (quote != default)
                {
                    if (format[pos] == quote)
                    {
                        sb.Append(format[quoteStart..(pos - quoteStart + 1)]);
                        quote = default;
                    }
                    pos++;
                    continue;
                }

                char c = format[pos];
                int next;
                switch (c)
                {
                    case '\\':
                        FlushSeparator(sb, ref separator);
                        sb.Append(++pos < length ? format[pos] : '\\');
                        break;
                    case '.' or '-' or '+' or ' ' or '_':
                        FlushSeparator(sb, ref separator);
                        separator = c;
                        break;
                    case '\'' or '"':
                        FlushSeparator(sb, ref separator);
                        quote = c;
                        quoteStart = pos + 1;
                        break;
                    case 'M':
                        next = pos + 1;
                        if (next < length && format[next] is 'M') // 'MM' - included if non-zero (?)
                        {
                            // pos = next; // consume extra character
                            throw new NotImplementedException("'MM' is a reserved identifier until I figure out what to do with it.");
                        }
                        else // 'M' - included always
                        {
                            FlushSeparator(sb, ref separator);
                            sb.SimpleAppend(Major);
                        }
                        break;
                    case 'm':
                        next = pos + 1;
                        if (next < length && format[next] is 'm') // 'mm' - included if non-zero
                        {
                            pos = next; // consume extra character
                            if (Minor > 0 || Patch > 0)
                            {
                                FlushSeparator(sb, ref separator);
                                sb.SimpleAppend(Minor);
                            }
                            else separator = default;
                        }
                        else // 'm' - included always
                        {
                            FlushSeparator(sb, ref separator);
                            sb.SimpleAppend(Minor);
                        }
                        break;
                    case 'p':
                        next = pos + 1;
                        if (next < length && format[next] is 'p') // 'pp' - included if non-zero
                        {
                            pos = next; // consume extra character
                            if (Patch > 0)
                            {
                                FlushSeparator(sb, ref separator);
                                sb.SimpleAppend(Patch);
                            }
                            else separator = default;
                        }
                        else // 'p' - included always
                        {
                            FlushSeparator(sb, ref separator);
                            sb.SimpleAppend(Patch);
                        }
                        break;
                    case 'r':
                        next = pos + 1;
                        if (next < length && format[next] is 'r') // 'rr' - include all pre-releases
                        {
                            pos = next; // consume extra character
                            int preReleasesLength = _preReleases.Length;
                            if (preReleasesLength > 0)
                            {
                                FlushSeparator(sb, ref separator);
                                sb.SimpleAppend(_preReleases[0]);
                                for (int i = 1; i < preReleasesLength; i++)
                                    sb.Append('.').SimpleAppend(_preReleases[i]);
                            }
                            else separator = default;
                        }
                        else // 'r' - indexed
                        {
                            throw new NotImplementedException();
                            // TODO: read a number after 'r' (if not specified, use 0, 1, 2 and so on)
                        }
                        break;
                    case 'd':
                        next = pos + 1;
                        if (next < length && format[next] is 'd') // 'dd' - include all metadata
                        {
                            pos = next; // consume extra character
                            int buildMetadataLength = _buildMetadata.Length;
                            if (buildMetadataLength > 0)
                            {
                                FlushSeparator(sb, ref separator);
                                sb.Append(_buildMetadata[0]);
                                for (int i = 1; i < buildMetadataLength; i++)
                                    sb.Append('.').Append(_buildMetadata[i]);
                            }
                            else separator = default;
                        }
                        else // 'd' - indexed
                        {
                            throw new NotImplementedException();
                            // TODO: read a number after 'd' (if not specified, use 0, 1, 2 and so on)
                        }
                        break;
                    default:
                        FlushSeparator(sb, ref separator);
                        sb.Append(c);
                        break;
                }
                pos++;
            }
            if (quote != default) throw new FormatException("The format string contains an unclosed quote ('\\'', '\"').");

            return sb.ToString();
        }
        public string ToString(string? format)
            => format is null ? ToString() : ToString(format.AsSpan());
        string IFormattable.ToString(string? format, IFormatProvider? provider)
            => ToString(format);

    }
}
