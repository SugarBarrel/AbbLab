using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AbbLab.Extensions;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public sealed partial class SemanticVersion : IEquatable<SemanticVersion>, IComparable, IComparable<SemanticVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        internal readonly SemanticPreRelease[] _preReleases;
        internal ReadOnlyCollection<SemanticPreRelease>? _preReleasesReadonly;
        public ReadOnlyCollection<SemanticPreRelease> PreReleases
        {
            get
            {
                if (_preReleasesReadonly is not null) return _preReleasesReadonly;
                return _preReleasesReadonly = _preReleases.Length is 0
                    ? ReadOnlyCollection.Empty<SemanticPreRelease>()
                    : new ReadOnlyCollection<SemanticPreRelease>(_preReleases);
            }
        }

        internal readonly string[] _buildMetadata;
        internal ReadOnlyCollection<string>? _buildMetadataReadonly;
        public ReadOnlyCollection<string> BuildMetadata
        {
            get
            {
                if (_buildMetadataReadonly is not null) return _buildMetadataReadonly;
                return _buildMetadataReadonly = _buildMetadata.Length is 0
                    ? ReadOnlyCollection.Empty<string>()
                    : new ReadOnlyCollection<string>(_buildMetadata);
            }
        }

        public bool IsPreRelease => _preReleases.Length > 0;

        // Internal constructor for performance reasons
        internal SemanticVersion(int major, int minor, int patch, SemanticPreRelease[]? preReleases, string[]? buildMetadata)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            _preReleases = preReleases ?? Array.Empty<SemanticPreRelease>();
            _buildMetadata = buildMetadata ?? Array.Empty<string>();
        }

        public SemanticVersion(int major, int minor, int patch)
            : this(major, minor, patch, (IEnumerable<SemanticPreRelease>?)null, null) { }
        public SemanticVersion(int major, int minor, int patch,
                               [InstantHandle] IEnumerable<SemanticPreRelease>? preReleases)
            : this(major, minor, patch, preReleases, null) { }
        public SemanticVersion(int major, int minor, int patch,
                               [InstantHandle] IEnumerable<SemanticPreRelease>? preReleases,
                               [InstantHandle] IEnumerable<string>? buildMetadata)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), major, Exceptions.MajorNegative);
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), minor, Exceptions.MinorNegative);
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), patch, Exceptions.PatchNegative);
            Major = major;
            Minor = minor;
            Patch = patch;

            SemanticPreRelease[] preReleasesArray;
            if (preReleases is not null && (preReleasesArray = preReleases.ToArray()).Length > 0)
                _preReleases = preReleasesArray;
            else _preReleases = Array.Empty<SemanticPreRelease>();

            string[] buildMetadataArray;
            if (buildMetadata is not null && (buildMetadataArray = buildMetadata.ToArray()).Length > 0)
            {
                for (int i = 0, length = buildMetadataArray.Length; i < length; i++)
                {
                    string identifier = buildMetadataArray[i];
                    Utility.ValidateBuildMetadata(identifier, nameof(buildMetadata));
                }
                _buildMetadata = buildMetadataArray;
            }
            else _buildMetadata = Array.Empty<string>();
        }

        public SemanticVersion(Version systemVersion)
        {
            Major = systemVersion.Major;
            Minor = systemVersion.Minor;
            Patch = Math.Max(systemVersion.Build, 0);
            _preReleases = Array.Empty<SemanticPreRelease>();
            _buildMetadata = Array.Empty<string>();
        }
        public SemanticVersion(PartialVersion partialVersion)
        {
            Major = partialVersion.Major.GetValueOrZero();
            Minor = partialVersion.Minor.GetValueOrZero();
            Patch = partialVersion.Patch.GetValueOrZero();
            _preReleases = partialVersion._preReleases;
            _preReleasesReadonly = partialVersion._preReleasesReadonly;
            _buildMetadata = partialVersion._buildMetadata;
            _buildMetadataReadonly = partialVersion._buildMetadataReadonly;
        }

        [Pure] [return: NotNullIfNotNull("systemVersion")]
        public static explicit operator SemanticVersion?(Version? systemVersion)
            => systemVersion is null ? null : new SemanticVersion(systemVersion);
        [Pure] [return: NotNullIfNotNull("version")]
        public static explicit operator Version?(SemanticVersion? version)
            => version is null ? null : new Version(version.Major, version.Minor, version.Patch);

        public static readonly SemanticVersion MinValue
            = new SemanticVersion(0, 0, 0, new SemanticPreRelease[1] { SemanticPreRelease.Zero });
        public static readonly SemanticVersion MaxValue
            = new SemanticVersion(int.MaxValue, int.MaxValue, int.MaxValue);

        [Pure] public bool Equals(SemanticVersion? other)
        {
            if (other is null || Major != other.Major || Minor != other.Minor || Patch != other.Patch) return false;

            int preReleasesLength = _preReleases.Length;
            if (preReleasesLength != other._preReleases.Length) return false;
            for (int i = 0; i < preReleasesLength; i++)
                if (!_preReleases[i].Equals(other._preReleases[i]))
                    return false;

            return true;
        }
        [Pure] public override bool Equals(object? obj)
            => Equals(obj as SemanticVersion);
        [Pure] public override int GetHashCode()
        {
            if (_preReleases.Length is 0) return HashCode.Combine(Major, Minor, Patch);

            HashCode hash = new HashCode();
            hash.Add(Major);
            hash.Add(Minor);
            hash.Add(Patch);

            int preReleasesLength = _preReleases.Length;
            for (int i = 0; i < preReleasesLength; i++)
                hash.Add(_preReleases[i]);

            return hash.ToHashCode();
        }

        [Pure] public int CompareTo(SemanticVersion? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            int res = Major.CompareTo(other.Major);
            if (res is not 0) return res;
            res = Minor.CompareTo(other.Minor);
            if (res is not 0) return res;
            res = Patch.CompareTo(other.Patch);
            if (res is not 0) return res;

            int thisLength = _preReleases.Length;
            int otherLength = other._preReleases.Length;
            if (thisLength is 0 && otherLength > 0) return 1;
            if (thisLength > 0 && otherLength is 0) return -1;

            for (int i = 0; ; i++)
            {
                if (i == thisLength) return i == otherLength ? 0 : -1;
                if (i == otherLength) return 1;
                res = _preReleases[i].CompareTo(other._preReleases[i]);
                if (res is not 0) return res;
            }
        }
        [Pure] int IComparable.CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is SemanticVersion other) return CompareTo(other);
            throw new ArgumentException($"Object must be of type {nameof(SemanticVersion)}", nameof(obj));
        }

        [Pure] public static bool operator ==(SemanticVersion? a, SemanticVersion? b) => a is null ? b is null : a.Equals(b);
        [Pure] public static bool operator !=(SemanticVersion? a, SemanticVersion? b) => a is null ? b is not null : !a.Equals(b);

        [Pure] public static bool operator >(SemanticVersion? a, SemanticVersion? b) => a is not null && a.CompareTo(b) > 0;
        [Pure] public static bool operator <(SemanticVersion? a, SemanticVersion? b) => a is null ? b is not null : a.CompareTo(b) < 0;
        [Pure] public static bool operator >=(SemanticVersion? a, SemanticVersion? b) => a is null ? b is null : a.CompareTo(b) >= 0;
        [Pure] public static bool operator <=(SemanticVersion? a, SemanticVersion? b) => a is null || a.CompareTo(b) <= 0;

    }
}
