namespace AbbLab.SemanticVersioning
{
    public abstract class Comparator
    {
        public abstract bool IsPrimitive { get; }
        public abstract bool Satisfies(SemanticVersion version);
        public abstract bool HasPreReleaseVersion(int major, int minor, int patch);

    }
}
