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

        public static implicit operator ComparatorSet(Comparator comparator) => new ComparatorSet(comparator);

        public static readonly ComparatorSet Empty = new ComparatorSet(Enumerable.Empty<Comparator>());

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
            // TODO: includePreReleases
            for (int i = 0, length = _comparators.Length; i < length; i++)
                if (!_comparators[i].Satisfies(version))
                    return false;
            return true;
        }
        public bool Satisfies(SemanticVersion version)
            => Satisfies(version, false);

        public static ComparatorSet operator &(ComparatorSet set, Comparator comparator)
            => new ComparatorSet(set.Comparators.Append(comparator));
        public static ComparatorSet operator &(Comparator comparator, ComparatorSet set)
            => new ComparatorSet(set.Comparators.Prepend(comparator));
        public static ComparatorSet operator &(ComparatorSet a, ComparatorSet b)
            => new ComparatorSet(a.Comparators.Concat(b.Comparators));

        public static VersionRange operator |(ComparatorSet set, Comparator comparator)
            => new VersionRange(set, new ComparatorSet(comparator));
        public static VersionRange operator |(Comparator comparator, ComparatorSet set)
            => new VersionRange(new ComparatorSet(comparator), set);
        public static VersionRange operator |(ComparatorSet a, ComparatorSet b)
            => new VersionRange(a, b);

        public ComparatorSet Simplify()
        {
            throw new NotImplementedException();
        }

    }
}