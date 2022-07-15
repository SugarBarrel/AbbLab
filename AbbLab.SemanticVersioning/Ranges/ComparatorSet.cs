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
        public bool Satisfies(SemanticVersion version) => Satisfies(version, false);

        public static ComparatorSet operator &(ComparatorSet a, Comparator b)
            => new ComparatorSet(a.Comparators.Append(b));
        public static ComparatorSet operator &(Comparator a, ComparatorSet b)
            => new ComparatorSet(b.Comparators.Prepend(a));
        public static ComparatorSet operator &(ComparatorSet a, ComparatorSet b)
            => new ComparatorSet(a.Comparators.Concat(b.Comparators));

        public static VersionRange operator |(ComparatorSet a, Comparator b)
            => new VersionRange(a, new ComparatorSet(b));
        public static VersionRange operator |(Comparator a, ComparatorSet b)
            => new VersionRange(new ComparatorSet(a), b);
        public static VersionRange operator |(ComparatorSet a, ComparatorSet b)
            => new VersionRange(a, b);

    }
}