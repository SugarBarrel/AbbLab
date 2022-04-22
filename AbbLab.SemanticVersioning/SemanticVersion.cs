﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly SemanticPreRelease[] _preReleases;
        private ReadOnlyCollection<SemanticPreRelease>? _preReleasesReadonly;
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

        private readonly string[] _buildMetadata;
        private ReadOnlyCollection<string>? _buildMetadataReadonly;
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
                    if (identifier.Length is 0)
                        throw new ArgumentException(Exceptions.BuildMetadataEmpty, nameof(buildMetadata));
                    if (!Utility.IsValidIdentifier(identifier))
                        throw new ArgumentException(Exceptions.BuildMetadataInvalid, nameof(buildMetadata));
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

        [Pure] public static explicit operator SemanticVersion(Version systemVersion) => new SemanticVersion(systemVersion);
        [Pure] public static explicit operator Version(SemanticVersion version) => new Version(version.Major, version.Minor, version.Patch);

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

            int maxLength = Math.Max(thisLength, otherLength);
            for (int i = 0; i < maxLength; i++)
            {
                if (i == thisLength) return i == otherLength ? 0 : -1;
                if (i == otherLength) return 1;
                res = _preReleases[i].CompareTo(other._preReleases[i]);
                if (res is not 0) return res;
            }

            return 0;
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