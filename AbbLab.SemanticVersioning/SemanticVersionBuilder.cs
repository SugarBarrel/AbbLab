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
                _buildMetadata.ForEach(static identifier => Utility.ValidateBuildMetadata(identifier, nameof(buildMetadata)));
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
            => AppendPreRelease(new SemanticPreRelease(identifier));
        public SemanticVersionBuilder AppendPreRelease(string identifier)
            => AppendPreRelease(SemanticPreRelease.Parse(identifier));
        public SemanticVersionBuilder AppendPreRelease(ReadOnlySpan<char> identifier)
            => AppendPreRelease(SemanticPreRelease.Parse(identifier));
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
            Utility.ValidateBuildMetadata(identifier, nameof(identifier));
            _buildMetadata.Add(identifier);
            return this;
        }
        public SemanticVersionBuilder AppendBuildMetadata(ReadOnlySpan<char> identifier)
        {
            Utility.ValidateBuildMetadata(identifier, nameof(identifier));
            _buildMetadata.Add(new string(identifier));
            return this;
        }

        public SemanticVersionBuilder ClearBuildMetadata()
        {
            _buildMetadata.Clear();
            return this;
        }

        public SemanticVersionBuilder IncrementMajor()
        {
            // 1.2.3-0 → 2.0.0
            // 1.0.0-0 → 1.0.0 | pre-release of a major release
            if (!(_minor is 0 && _patch is 0 && _preReleases.Count > 0))
            {
                if (_major is int.MaxValue) throw new InvalidOperationException(Exceptions.MajorTooBig);
                _major++;
            }
            _minor = 0;
            _patch = 0;
            _preReleases.Clear();
            return this;
        }
        public SemanticVersionBuilder IncrementMinor()
        {
            // 1.2.3-0 → 1.3.0
            // 1.2.0-0 → 1.2.0 | pre-release of a minor release
            if (!(_patch is 0 && _preReleases.Count > 0))
            {
                if (_minor is int.MaxValue) throw new InvalidOperationException(Exceptions.MinorTooBig);
                _minor++;
            }
            _patch = 0;
            _preReleases.Clear();
            return this;
        }
        public SemanticVersionBuilder IncrementPatch()
        {
            // 1.2.3   → 1.2.4
            // 1.2.3-0 → 1.2.3 | pre-release of a patch release
            if (_preReleases.Count is 0)
            {
                if (_patch is int.MaxValue) throw new InvalidOperationException(Exceptions.PatchTooBig);
                _patch++;
            }
            _preReleases.Clear();
            return this;
        }

        public SemanticVersionBuilder IncrementPreMajor()
            => IncrementPreMajor(SemanticPreRelease.Zero);
        public SemanticVersionBuilder IncrementPreMajor(int preRelease)
            => IncrementPreMajor(new SemanticPreRelease(preRelease));
        public SemanticVersionBuilder IncrementPreMajor(string? preRelease)
            => IncrementPreMajor(preRelease is null ? SemanticPreRelease.Zero : SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreMajor(ReadOnlySpan<char> preRelease)
            => IncrementPreMajor(SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreMajor(SemanticPreRelease preRelease)
        {
            if (_major is int.MaxValue) throw new InvalidOperationException(Exceptions.MajorTooBig);
            _major++;
            _minor = 0;
            _patch = 0;

            // 1.2.3 →   (0)   → 2.0.0-0       | 0 specifies not to use an extra identifier
            // 1.2.3 →   (1)   → 2.0.0-1.0
            // 1.2.3 → (alpha) → 2.0.0-alpha.0
            _preReleases.Clear();
            _preReleases.Add(preRelease);
            if (preRelease != SemanticPreRelease.Zero)
                _preReleases.Add(SemanticPreRelease.Zero);

            return this;
        }

        public SemanticVersionBuilder IncrementPreMinor()
            => IncrementPreMinor(SemanticPreRelease.Zero);
        public SemanticVersionBuilder IncrementPreMinor(int preRelease)
            => IncrementPreMinor(new SemanticPreRelease(preRelease));
        public SemanticVersionBuilder IncrementPreMinor(string? preRelease)
            => IncrementPreMinor(preRelease is null ? SemanticPreRelease.Zero : SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreMinor(ReadOnlySpan<char> preRelease)
            => IncrementPreMinor(SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreMinor(SemanticPreRelease preRelease)
        {
            if (_minor is int.MaxValue) throw new InvalidOperationException(Exceptions.MinorTooBig);
            _minor++;
            _patch = 0;

            // 1.2.3 →   (0)   → 1.3.0-0       | 0 specifies not to use an extra identifier
            // 1.2.3 →   (1)   → 1.3.0-1.0
            // 1.2.3 → (alpha) → 1.3.0-alpha.0
            _preReleases.Clear();
            _preReleases.Add(preRelease);
            if (preRelease != SemanticPreRelease.Zero)
                _preReleases.Add(SemanticPreRelease.Zero);

            return this;
        }

        public SemanticVersionBuilder IncrementPrePatch()
            => IncrementPrePatch(SemanticPreRelease.Zero);
        public SemanticVersionBuilder IncrementPrePatch(int preRelease)
            => IncrementPrePatch(new SemanticPreRelease(preRelease));
        public SemanticVersionBuilder IncrementPrePatch(string? preRelease)
            => IncrementPrePatch(preRelease is null ? SemanticPreRelease.Zero : SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPrePatch(ReadOnlySpan<char> preRelease)
            => IncrementPrePatch(SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPrePatch(SemanticPreRelease preRelease)
        {
            if (_patch is int.MaxValue) throw new InvalidOperationException(Exceptions.PatchTooBig);
            _patch++;

            // 1.2.3 →   (0)   → 1.2.4-0       | 0 specifies not to use an extra identifier
            // 1.2.3 →   (1)   → 1.2.4-1.0
            // 1.2.3 → (alpha) → 1.2.4-alpha.0
            _preReleases.Clear();
            _preReleases.Add(preRelease);
            if (preRelease != SemanticPreRelease.Zero)
                _preReleases.Add(SemanticPreRelease.Zero);

            return this;
        }

        public SemanticVersionBuilder IncrementPreRelease()
            => IncrementPreRelease(SemanticPreRelease.Zero);
        public SemanticVersionBuilder IncrementPreRelease(int preRelease)
            => IncrementPreRelease(new SemanticPreRelease(preRelease));
        public SemanticVersionBuilder IncrementPreRelease(string? preRelease)
            => IncrementPreRelease(preRelease is null ? SemanticPreRelease.Zero : SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreRelease(ReadOnlySpan<char> preRelease)
            => IncrementPreRelease(SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder IncrementPreRelease(SemanticPreRelease preRelease)
        {
            if (_preReleases.Count is 0)
            {
                // increment patch and add 'pre.0' or '0'
                IncrementPrePatch(preRelease);
            }
            else if (preRelease == SemanticPreRelease.Zero || preRelease == _preReleases[0])
            {
                // try to increment the right-most numeric identifier
                int i;
                for (i = _preReleases.Count - 1; i >= 0; i--)
                {
                    SemanticPreRelease identifier = _preReleases[i];
                    if (identifier.IsNumeric)
                    {
                        int number = identifier.Number;
                        if (number is int.MaxValue) throw new InvalidOperationException(Exceptions.PreReleaseTooBig);
                        _preReleases[i] = new SemanticPreRelease(number + 1);
                        break;
                    }
                }
                if (i is -1) // couldn't find a numeric identifier
                    _preReleases.Add(SemanticPreRelease.Zero);
            }
            else
            {
                // replace the pre-releases with 'pre.0'
                _preReleases.Clear();
                _preReleases.Add(preRelease);
                _preReleases.Add(SemanticPreRelease.Zero);
            }
            return this;
        }

        public SemanticVersionBuilder Increment(IncrementType type)
            => Increment(type, SemanticPreRelease.Zero);
        public SemanticVersionBuilder Increment(IncrementType type, int preRelease)
            => Increment(type, new SemanticPreRelease(preRelease));
        public SemanticVersionBuilder Increment(IncrementType type, string? preRelease)
            => Increment(type, preRelease is null ? SemanticPreRelease.Zero : SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder Increment(IncrementType type, ReadOnlySpan<char> preRelease)
            => Increment(type, SemanticPreRelease.Parse(preRelease));
        public SemanticVersionBuilder Increment(IncrementType type, SemanticPreRelease preRelease) => type switch
        {
            IncrementType.None => this,

            IncrementType.Major => IncrementMajor(),
            IncrementType.Minor => IncrementMinor(),
            IncrementType.Patch => IncrementPatch(),

            IncrementType.PreMajor => IncrementPreMajor(preRelease),
            IncrementType.PreMinor => IncrementPreMinor(preRelease),
            IncrementType.PrePatch => IncrementPrePatch(preRelease),

            IncrementType.PreRelease => IncrementPreRelease(preRelease),

            _ => throw new ArgumentException($"Invalid {nameof(IncrementType)} value.", nameof(type)),
        };

        [Pure] public SemanticVersion ToVersion()
        {
            SemanticPreRelease[] preReleases = _preReleases.Count > 0 ? _preReleases.ToArray() : Array.Empty<SemanticPreRelease>();
            string[] buildMetadata = _buildMetadata.Count > 0 ? _buildMetadata.ToArray() : Array.Empty<string>();
            return new SemanticVersion(_major, _minor, _patch, preReleases, buildMetadata);
        }

    }
}
