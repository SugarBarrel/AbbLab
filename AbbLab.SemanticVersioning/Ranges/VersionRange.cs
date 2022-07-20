using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AbbLab.Extensions;

namespace AbbLab.SemanticVersioning
{
    public sealed class VersionRange
    {
        public VersionRange(params ComparatorSet[] comparatorSets) : this(comparatorSets.AsEnumerable()) { }
        public VersionRange(IEnumerable<ComparatorSet> comparatorSets)
        {
            _comparatorSets = comparatorSets.ToArray();
            if (_comparatorSets.Length is 0)
                throw new ArgumentException("The version range must contain at least one comparator.", nameof(comparatorSets));
        }

        public static readonly VersionRange Empty = new VersionRange(ComparatorSet.Empty);
        public static readonly VersionRange All = new VersionRange(ComparatorSet.All);

        public static implicit operator VersionRange(Comparator comparator) => new VersionRange(new ComparatorSet(comparator));
        public static implicit operator VersionRange(ComparatorSet comparatorSet) => new VersionRange(comparatorSet);

        private readonly ComparatorSet[] _comparatorSets;
        private ReadOnlyCollection<ComparatorSet>? _comparatorSetsReadonly;
        public ReadOnlyCollection<ComparatorSet> ComparatorSets
        {
            get
            {
                if (_comparatorSetsReadonly is not null) return _comparatorSetsReadonly;
                return _comparatorSetsReadonly = _comparatorSets.Length is 0
                    ? ReadOnlyCollection.Empty<ComparatorSet>()
                    : new ReadOnlyCollection<ComparatorSet>(_comparatorSets);
            }
        }

        public bool Satisfies(SemanticVersion version, bool includePreReleases)
            => Array.Exists(_comparatorSets, cs => cs.Satisfies(version, includePreReleases));
        public bool Satisfies(SemanticVersion version)
            => Satisfies(version, false);

        public VersionRange Simplify()
        {
            throw new NotImplementedException();
        }

    }
}