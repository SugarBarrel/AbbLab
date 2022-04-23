using System;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public readonly partial struct SemanticPreRelease : IEquatable<SemanticPreRelease>, IComparable, IComparable<SemanticPreRelease>
    {
        private readonly string? text;
        private readonly int number;

        public SemanticPreRelease(int identifier)
        {
            if (identifier < 0) throw new ArgumentOutOfRangeException(nameof(identifier), identifier, Exceptions.PreReleaseNegative);
            text = null;
            number = identifier;
        }
        public SemanticPreRelease(string identifier) => this = Parse(identifier);
        public SemanticPreRelease(ReadOnlySpan<char> identifier) => this = Parse(identifier);

        [Pure] public static implicit operator SemanticPreRelease(int identifier) => new SemanticPreRelease(identifier);
        [Pure] public static implicit operator SemanticPreRelease(string identifier) => Parse(identifier);
        [Pure] public static implicit operator SemanticPreRelease(ReadOnlySpan<char> identifier) => Parse(identifier);
        [Pure] public static explicit operator int(SemanticPreRelease preRelease) => preRelease.Number;
        [Pure] public static explicit operator string(SemanticPreRelease preRelease) => preRelease.Text;
        [Pure] public static explicit operator ReadOnlySpan<char>(SemanticPreRelease preRelease) => preRelease.Text.AsSpan();

        public bool IsNumeric => text is null;
        public string Text => text ?? throw new InvalidOperationException(Exceptions.PreReleaseNotAlphanumeric);
        public int Number => text is null ? number : throw new InvalidOperationException(Exceptions.PreReleaseNotNumeric);

        public static readonly SemanticPreRelease Zero = new SemanticPreRelease(0);

        [Pure] public bool Equals(SemanticPreRelease other)
            => text is null ? other.text is null && number == other.number : text == other.text;
        [Pure] public override bool Equals(object? obj)
            => obj is SemanticPreRelease other && Equals(other);
        [Pure] public override int GetHashCode()
            => text?.GetHashCode() ?? number;

        [Pure] public int CompareTo(SemanticPreRelease other)
        {
            bool isNumeric = text is null;
            if (isNumeric ^ other.text is null) return isNumeric ? -1 : 1;
            return isNumeric ? number.CompareTo(other.number) : string.CompareOrdinal(text, other.text);
        }
        [Pure] int IComparable.CompareTo(object? obj)
        {
            if (obj is SemanticPreRelease other) return CompareTo(other);
            throw new ArgumentException($"Object must be of type {nameof(SemanticPreRelease)}", nameof(obj));
        }

        [Pure] public static bool operator ==(SemanticPreRelease a, SemanticPreRelease b) => a.Equals(b);
        [Pure] public static bool operator !=(SemanticPreRelease a, SemanticPreRelease b) => !a.Equals(b);

        [Pure] public static bool operator >(SemanticPreRelease a, SemanticPreRelease b) => a.CompareTo(b) > 0;
        [Pure] public static bool operator <(SemanticPreRelease a, SemanticPreRelease b) => a.CompareTo(b) < 0;
        [Pure] public static bool operator >=(SemanticPreRelease a, SemanticPreRelease b) => a.CompareTo(b) >= 0;
        [Pure] public static bool operator <=(SemanticPreRelease a, SemanticPreRelease b) => a.CompareTo(b) <= 0;

    }
}
