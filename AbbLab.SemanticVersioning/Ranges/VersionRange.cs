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
            => _comparatorSets = comparatorSets.ToArray();

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
        {
            int length = _comparatorSets.Length;
            for (int i = 0; i < length; i++)
                if (_comparatorSets[i].Satisfies(version, includePreReleases))
                    return true;
            return length is not 0;
        }
        public bool Satisfies(SemanticVersion version)
            => Satisfies(version, false);

        public static VersionRange operator &(VersionRange range, ComparatorSet set)
            => new VersionRange(range.ComparatorSets.Select(s => s & set));
        public static VersionRange operator &(ComparatorSet set, VersionRange range)
            => new VersionRange(range.ComparatorSets.Select(s => set & s));
        public static VersionRange operator &(VersionRange range, Comparator comparator)
            => new VersionRange(range.ComparatorSets.Select(s => s & comparator));
        public static VersionRange operator &(Comparator comparator, VersionRange range)
            => new VersionRange(range.ComparatorSets.Select(s => comparator & s));

        public static VersionRange operator &(VersionRange a, VersionRange b)
            => new VersionRange(a.ComparatorSets.SelectMany(setA => b.ComparatorSets.Select(setB => setA & setB)));

        public static VersionRange operator |(VersionRange range, ComparatorSet set)
            => new VersionRange(range.ComparatorSets.Append(set));
        public static VersionRange operator |(ComparatorSet set, VersionRange range)
            => new VersionRange(range.ComparatorSets.Prepend(set));
        public static VersionRange operator |(VersionRange range, Comparator comparator)
            => new VersionRange(range.ComparatorSets.Append(new ComparatorSet(comparator)));
        public static VersionRange operator |(Comparator comparator, VersionRange range)
            => new VersionRange(range.ComparatorSets.Prepend(new ComparatorSet(comparator)));

        public static VersionRange operator |(VersionRange a, VersionRange b)
            => new VersionRange(a.ComparatorSets.Concat(b.ComparatorSets));

        public VersionRange Simplify()
        {
            throw new NotImplementedException();
        }

    }
}