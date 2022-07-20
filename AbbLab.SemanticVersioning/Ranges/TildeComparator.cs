using static AbbLab.SemanticVersioning.PrimitiveComparator;

namespace AbbLab.SemanticVersioning
{
    public sealed class TildeComparator : AdvancedComparator
    {
        public TildeComparator(PartialVersion version) : base(version) { }

        protected override (PrimitiveComparator?, PrimitiveComparator?) ConvertToPrimitives()
        {
            // ~, ~x.x.x ⇒ *
            if (!Operand.Major.IsNumeric) return (null, null);
            int major = Operand.Major.Number;

            if (!Operand.Minor.IsNumeric)
            {
                // >=M.0.0 <M+1.0.0-0
                // ~1, ~1.x.x ⇒ >=1.0.0 <2.0.0-0
                return (
                    GreaterThanOrEqual(new SemanticVersion(major, 0, 0, null, null)),
                    LessThan(new SemanticVersion(major + 1, 0, 0, SemanticPreRelease.ZeroArray, null))
                ); // TODO: check for overflow
            }
            int minor = Operand.Minor.Number;
            int patch = Operand.Patch.GetValueOrZero();

            // >=M.m.p[-rr] <M.m+1.0-0
            // ~1.2, ~1.2.x ⇒ >=1.2.0 <1.3.0-0
            // ~1.2.3-rc ⇒ >=1.2.3-rc <1.3.0-0
            return (
                GreaterThanOrEqual(new SemanticVersion(major, minor, patch, Operand._preReleases, null)),
                LessThan(new SemanticVersion(major, minor + 1, 0, SemanticPreRelease.ZeroArray, null))
            ); // TODO: check for overflow

        }

    }
}
