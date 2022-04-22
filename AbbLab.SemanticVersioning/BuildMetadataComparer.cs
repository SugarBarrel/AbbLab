using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public sealed class BuildMetadataComparer : IComparer, IComparer<SemanticVersion?>,
                                                IEqualityComparer, IEqualityComparer<SemanticVersion?>
    {
        private BuildMetadataComparer() { }

        public static BuildMetadataComparer Instance { get; } = new BuildMetadataComparer();

        [Pure] public int Compare(SemanticVersion? a, SemanticVersion? b)
        {
            if (a is null) return b is null ? 0 : -1;

            int res = a.CompareTo(b);
            if (res is not 0) return res;

            // Build metadata comparison is identical to the pre-release one
            int aLength = a._buildMetadata.Length;
            int bLength = b!._buildMetadata.Length;
            if (aLength is 0 && bLength > 0) return 1;
            if (aLength > 0 && bLength is 0) return -1;

            int maxLength = Math.Max(aLength, bLength);
            for (int i = 0; i < maxLength; i++)
            {
                if (i == aLength) return i == bLength ? 0 : -1;
                if (i == bLength) return 1;
                res = string.CompareOrdinal(a._buildMetadata[i], b._buildMetadata[i]);
                if (res is not 0) return res;
            }

            return 0;
        }
        [Pure] int IComparer.Compare(object? a, object? b)
        {
            if (a is null) return b is null ? 0 : -1;
            if (b is null) return 1;

            if (a is SemanticVersion versionA && b is SemanticVersion versionB)
                return Compare(versionA, versionB);

            throw new ArgumentException($"The objects must be of type {nameof(SemanticVersion)}.");
        }

        [Pure] public bool Equals(SemanticVersion? a, SemanticVersion? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a != b) return false;

            int buildMetadataLength = a!._buildMetadata.Length;
            if (buildMetadataLength != b!._buildMetadata.Length) return false;
            for (int i = 0; i < buildMetadataLength; i++)
                if (a._buildMetadata[i] != b._buildMetadata[i])
                    return false;

            return true;
        }
        [Pure] bool IEqualityComparer.Equals(object? a, object? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            if (a is SemanticVersion versionA && b is SemanticVersion versionB)
                return Equals(versionA, versionB);

            throw new ArgumentException($"The objects must be of type {nameof(SemanticVersion)}.");
        }

        [Pure] public int GetHashCode(SemanticVersion? version)
        {
            if (version is null) return 0;
            if (version._buildMetadata.Length is 0) return version.GetHashCode();

            HashCode hash = new HashCode();
            hash.Add(version);

            int buildMetadataLength = version._buildMetadata.Length;
            for (int i = 0; i < buildMetadataLength; i++)
                hash.Add(version._buildMetadata[i]);

            return hash.ToHashCode();
        }
        [Pure] int IEqualityComparer.GetHashCode(object? obj)
        {
            if (obj is null) return 0;
            if (obj is SemanticVersion version) return GetHashCode(version);

            throw new ArgumentException($"The object must be of type {nameof(SemanticVersion)}.", nameof(obj));
        }

    }
}
