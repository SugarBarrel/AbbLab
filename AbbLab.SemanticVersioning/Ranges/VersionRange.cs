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

        public bool Satisfies(SemanticVersion version)
        {
            int length = _comparatorSets.Length;
            for (int i = 0; i < length; i++)
                if (_comparatorSets[i].Satisfies(version))
                    return true;
            return length is not 0;
        }

        public static VersionRange operator |(VersionRange a, ComparatorSet b)
            => new VersionRange(a.ComparatorSets.Append(b));
        public static VersionRange operator |(ComparatorSet a, VersionRange b)
            => new VersionRange(b.ComparatorSets.Prepend(a));
        public static VersionRange operator |(VersionRange a, VersionRange b)
            => new VersionRange(a.ComparatorSets.Concat(b.ComparatorSets));

    }
}