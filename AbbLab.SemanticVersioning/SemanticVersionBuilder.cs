using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    public sealed class SemanticVersionBuilder
    {
        private int _major;
        private int _minor;
        private int _patch;
        private readonly List<SemanticPreRelease> _preReleases;
        private readonly List<string> _buildMetadata;
        private ReadOnlyCollection<SemanticPreRelease>? _preReleasesReadonly;
        private ReadOnlyCollection<string>? _buildMetadataReadonly;

        public int Major
        {
            get => _major;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, Exceptions.MajorNegative);
                _major = value;
            }
        }
        public int Minor
        {
            get => _minor;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, Exceptions.MinorNegative);
                _minor = value;
            }
        }
        public int Patch
        {
            get => _patch;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, Exceptions.PatchNegative);
                _patch = value;
            }
        }

        public ReadOnlyCollection<SemanticPreRelease> PreReleases
            => _preReleasesReadonly ??= new ReadOnlyCollection<SemanticPreRelease>(_preReleases);
        public ReadOnlyCollection<string> BuildMetadata
            => _buildMetadataReadonly ??= new ReadOnlyCollection<string>(_buildMetadata);

        public SemanticVersionBuilder()
            : this(0, 0, 0, null, null) { }
        public SemanticVersionBuilder(int major, int minor, int patch)
            : this(major, minor, patch, null, null) { }
        public SemanticVersionBuilder(int major, int minor, int patch,
                                      [InstantHandle] IEnumerable<SemanticPreRelease>? preReleases)
            : this(major, minor, patch, preReleases, null) { }
        public SemanticVersionBuilder(int major, int minor, int patch,
                                      [InstantHandle] IEnumerable<SemanticPreRelease>? preReleases,
                                      [InstantHandle] IEnumerable<string>? buildMetadata)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), major, Exceptions.MajorNegative);
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), minor, Exceptions.MinorNegative);
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), patch, Exceptions.PatchNegative);
            _major = major;
            _minor = minor;
            _patch = patch;

            _preReleases = preReleases is null ? new List<SemanticPreRelease>() : new List<SemanticPreRelease>(preReleases);

            if (buildMetadata is not null)
            {
                _buildMetadata = new List<string>(buildMetadata);
                _buildMetadata.ForEach(static identifier =>
                {
                    if (identifier.Length is 0)
                        throw new ArgumentException(Exceptions.BuildMetadataEmpty, nameof(buildMetadata));
                    if (!Utility.IsValidIdentifier(identifier))
                        throw new ArgumentException(Exceptions.BuildMetadataInvalid, nameof(buildMetadata));
                });
            }
            else _buildMetadata = new List<string>();
        }

        public SemanticVersionBuilder WithMajor(int major)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), major, Exceptions.MajorNegative);
            _major = major;
            return this;
        }
        public SemanticVersionBuilder WithMinor(int minor)
        {
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), minor, Exceptions.MinorNegative);
            _minor = minor;
            return this;
        }
        public SemanticVersionBuilder WithPatch(int patch)
        {
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), patch, Exceptions.PatchNegative);
            _patch = patch;
            return this;
        }

        public SemanticVersionBuilder AppendPreRelease(int identifier)
        {
            _preReleases.Add(new SemanticPreRelease(identifier));
            return this;
        }
        public SemanticVersionBuilder AppendPreRelease(string identifier)
        {
            _preReleases.Add(SemanticPreRelease.Parse(identifier));
            return this;
        }
        public SemanticVersionBuilder AppendPreRelease(ReadOnlySpan<char> identifier)
        {
            _preReleases.Add(SemanticPreRelease.Parse(identifier));
            return this;
        }
        public SemanticVersionBuilder AppendPreRelease(SemanticPreRelease identifier)
        {
            _preReleases.Add(identifier);
            return this;
        }

        public SemanticVersionBuilder ClearPreReleases()
        {
            _preReleases.Clear();
            return this;
        }

        public SemanticVersionBuilder AppendBuildMetadata(string identifier)
        {
            if (identifier.Length is 0)
                throw new ArgumentException(Exceptions.BuildMetadataEmpty, nameof(identifier));
            if (!Utility.IsValidIdentifier(identifier))
                throw new ArgumentException(Exceptions.BuildMetadataInvalid, nameof(identifier));
            _buildMetadata.Add(identifier);
            return this;
        }
        public SemanticVersionBuilder AppendBuildMetadata(ReadOnlySpan<char> identifier)
        {
            if (identifier.Length is 0)
                throw new ArgumentException(Exceptions.BuildMetadataEmpty, nameof(identifier));
            if (!Utility.IsValidIdentifier(identifier))
                throw new ArgumentException(Exceptions.BuildMetadataInvalid, nameof(identifier));
            _buildMetadata.Add(new string(identifier));
            return this;
        }

        public SemanticVersionBuilder ClearBuildMetadata()
        {
            _buildMetadata.Clear();
            return this;
        }

        // TODO: Increment methods

        [Pure] public SemanticVersion ToVersion()
        {
            SemanticPreRelease[] preReleases = _preReleases.Count > 0 ? _preReleases.ToArray() : Array.Empty<SemanticPreRelease>();
            string[] buildMetadata = _buildMetadata.Count > 0 ? _buildMetadata.ToArray() : Array.Empty<string>();
            return new SemanticVersion(_major, _minor, _patch, preReleases, buildMetadata);
        }

    }
}
