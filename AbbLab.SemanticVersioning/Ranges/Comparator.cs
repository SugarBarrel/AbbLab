namespace AbbLab.SemanticVersioning
{
    public abstract class Comparator
    {
        public abstract bool IsPrimitive { get; }
        public abstract bool Satisfies(SemanticVersion version);

        public static ComparatorSet operator &(Comparator a, Comparator b)
            => new ComparatorSet(a, b);
        public static VersionRange operator |(Comparator a, Comparator b)
            => new VersionRange(new ComparatorSet(a), new ComparatorSet(b));

    }
}
