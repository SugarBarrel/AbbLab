using static AbbLab.SemanticVersioning.PrimitiveComparator;

namespace AbbLab.SemanticVersioning
{
    public sealed class CaretComparator : AdvancedComparator
    {
        public CaretComparator(PartialVersion version) : base(version) { }

        protected override (PrimitiveComparator?, PrimitiveComparator?) ConvertToPrimitives()
        {
            // ^, ^x.x.x ⇒ *
            if (!Operand.Major.IsNumeric) return (null, null);
            int major = Operand.Major.Number; // M is numeric

            if (major is not 0 || !Operand.Minor.IsNumeric)
            {
                // >=M.m.p[-rr] <M+1.0.0-0
                // ^1.2.3 ⇒ >=1.2.3 <2.0.0-0
                // ^1, ^1.x.x ⇒ >=1.0.0 <2.0.0-0
                // ^0, ^0.x.x ⇒ >=0.0.0 <1.0.0-0
                // ^0.x.3 ⇒ >=0.0.3 <1.0.0-0 TODO: not in node-semver
                return (
                    GreaterThanOrEqual(new SemanticVersion(major, Operand.Minor.GetValueOrZero(), Operand.Patch.GetValueOrZero(), null, null)),
                    LessThan(new SemanticVersion(major + 1, 0, 0, SemanticPreRelease.ZeroArray, null))
                ); // TODO: check for overflow
            }
            int minor = Operand.Minor.Number; // M is 0, m is numeric

            if (minor is not 0 || !Operand.Patch.IsNumeric)
            {
                // >=0.m.p[-rr] <0.m+1.0-0
                // ^0.1.2 ⇒ >=0.1.2 <0.2.0-0
                // ^0.1, ^0.1.x ⇒ >=0.1.0 <0.2.0-0
                // ^0.0, ^0.0.x ⇒ >=0.0.0 <0.1.0-0
                return (
                    GreaterThanOrEqual(new SemanticVersion(0, minor, Operand.Patch.GetValueOrZero(), null, null)),
                    LessThan(new SemanticVersion(0, minor + 1, 0, SemanticPreRelease.ZeroArray, null))
                ); // TODO: check for overflow
            }
            int patch = Operand.Patch.Number; // M is 0, m is 0, p is numeric

            // >=0.0.p[-rr] <0.0.p+1-0
            // ^0.0.1 ⇒ >=0.0.1 <0.0.2-0
            // ^0.0.0 ⇒ >=0.0.0 <0.0.1-0
            return (
                GreaterThanOrEqual(new SemanticVersion(0, 0, patch, null, null)),
                LessThan(new SemanticVersion(0, 0, patch + 1, SemanticPreRelease.ZeroArray, null))
            ); // TODO: check for overflow

        }
    }
}
