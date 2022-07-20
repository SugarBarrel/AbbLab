using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AbbLab.Extensions;

namespace AbbLab.SemanticVersioning
{
    public sealed class ComparatorSet
    {
        public ComparatorSet(params Comparator[] comparators) : this(comparators.AsEnumerable()) { }
        public ComparatorSet(IEnumerable<Comparator> comparators)
            => _comparators = comparators.ToArray();

        public static readonly ComparatorSet Empty = new ComparatorSet(PrimitiveComparator.LessThan(SemanticVersion.MinValue));
        public static readonly ComparatorSet All = new ComparatorSet(PrimitiveComparator.GreaterThanOrEqual(SemanticVersion.MinValue));

        public static implicit operator ComparatorSet(Comparator comparator) => new ComparatorSet(comparator);

        private readonly Comparator[] _comparators;
        private ReadOnlyCollection<Comparator>? _comparatorsReadonly;
        public ReadOnlyCollection<Comparator> Comparators
        {
            get
            {
                if (_comparatorsReadonly is not null) return _comparatorsReadonly;
                return _comparatorsReadonly = _comparators.Length is 0
                    ? ReadOnlyCollection.Empty<Comparator>()
                    : new ReadOnlyCollection<Comparator>(_comparators);
            }
        }

        public bool Satisfies(SemanticVersion version, bool includePreReleases)
        {
            if (!includePreReleases && version.IsPreRelease)
            {
                bool canCompare = Array.Exists(_comparators, c => c.HasPreReleaseVersion(version.Major, version.Minor, version.Patch));
                if (!canCompare) return false;
            }
            return Array.TrueForAll(_comparators, c => c.Satisfies(version));
        }
        public bool Satisfies(SemanticVersion version)
            => Satisfies(version, false);

    }
}