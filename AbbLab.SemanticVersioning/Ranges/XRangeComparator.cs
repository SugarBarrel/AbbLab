using System;

namespace AbbLab.SemanticVersioning
{
    public sealed class XRangeComparator : AdvancedComparator
    {
        public XRangeComparator(PartialVersion version) : this(version, PrimitiveOperator.Equal) { }
        public XRangeComparator(PartialVersion version, PrimitiveOperator @operator) : base(version)
        {
            if (@operator is not PrimitiveOperator.GreaterThan and not PrimitiveOperator.GreaterThanOrEqual
                and not PrimitiveOperator.LessThan and not PrimitiveOperator.LessThanOrEqual and not PrimitiveOperator.Equal)
                throw new ArgumentException($"{@operator} is not a valid primitive operator.", nameof(@operator));
            Operator = @operator;
        }
        public PrimitiveOperator Operator { get; }

        protected override (PrimitiveComparator?, PrimitiveComparator?) ConvertToPrimitives()
        {
            if (!Operand.Major.IsNumeric)
            {
                if (Operator is PrimitiveOperator.GreaterThan or PrimitiveOperator.LessThan)
                    return (PrimitiveComparator.LessThan(SemanticVersion.MinValue), null);
                return (null, null);
            }
            if (Operand.Minor.IsNumeric && Operand.Patch.IsNumeric)
                return (new PrimitiveComparator(new SemanticVersion(Operand), Operator), null);

            int major = Operand.Major.Number;
            switch (Operator)
            {
                case PrimitiveOperator.GreaterThan:
                    // >1.2, >1.2.x ⇒ >=1.3.0-0
                    // >1.2.x-rc ⇒ >=1.3.0-0 (pre-releases don't matter)
                    // >1, >1.x.x ⇒ >=2.0.0-0
                    if (Operand.Minor.IsNumeric)
                    {
                        return (PrimitiveComparator.GreaterThanOrEqual(
                                    new SemanticVersion(major, Operand.Minor.Number + 1, 0, SemanticPreRelease.ZeroArray, null)),
                                null); // TODO: check for overflow
                    }
                    return (PrimitiveComparator.GreaterThanOrEqual(
                                new SemanticVersion(major + 1, 0, 0, SemanticPreRelease.ZeroArray, null)),
                            null); // TODO: check for overflow

                case PrimitiveOperator.LessThanOrEqual:
                    // <=1.2, <=1.2.x ⇒ <1.3.0-0
                    // <=1.2.x-rc ⇒ <1.3.0-0 (pre-releases don't matter)
                    // <=1, <=1.x.x ⇒ <2.0.0-0
                    if (Operand.Minor.IsNumeric)
                    {
                        return (PrimitiveComparator.LessThan(
                                    new SemanticVersion(major, Operand.Minor.Number + 1, 0, SemanticPreRelease.ZeroArray, null)),
                                null); // TODO: check for overflow
                    }
                    return (PrimitiveComparator.LessThan(
                                new SemanticVersion(major + 1, 0, 0, SemanticPreRelease.ZeroArray, null)),
                            null); // TODO: check for overflow

                case PrimitiveOperator.GreaterThanOrEqual:
                    // >=1.2, >=1.2.x ⇒ >=1.2.0
                    // >=1.2.x-rc ⇒ >=1.2.0-rc (pre-releases carry over)
                    // >=1, >=1.x.x ⇒ >=1.0.0
                    // >=1.x.x-rc ⇒ >=1.0.0-rc
                    if (Operand.Minor.IsNumeric)
                    {
                        return (PrimitiveComparator.GreaterThanOrEqual(
                                    new SemanticVersion(major, Operand.Minor.Number, 0, Operand._preReleases, null)),
                                null);
                    }
                    return (PrimitiveComparator.GreaterThanOrEqual(
                                new SemanticVersion(major, 0, 0, Operand._preReleases, null)),
                            null);

                case PrimitiveOperator.LessThan:
                    // <1.2, <1.2.x ⇒ <1.2.0
                    // <1.2.x-rc ⇒ <1.2.0-rc (pre-releases carry over)
                    // <1, <1.x.x ⇒ <1.0.0
                    // <1.x.x-rc ⇒ <1.0.0-rc
                    if (Operand.Minor.IsNumeric)
                    {
                        return (PrimitiveComparator.LessThan(
                                    new SemanticVersion(major, Operand.Minor.Number, 0, Operand._preReleases, null)),
                                null);
                    }
                    return (PrimitiveComparator.LessThan(
                                new SemanticVersion(major, 0, 0, Operand._preReleases, null)),
                            null);

                case PrimitiveOperator.Equal:
                    // =1.2, =1.2.x ⇒ >=1.2.0 <1.3.0-0
                    // =1.2.x-rc ⇒ >=1.2.0-rc <1.3.0-0 (mixed)
                    // =1, =1.x.x ⇒ >=1.0.0 <2.0.0-0
                    // =1.x.x-rc ⇒ >=1.0.0-rc <2.0.0-0
                    if (Operand.Minor.IsNumeric)
                    {
                        int minor = Operand.Minor.Number;
                        return (
                            PrimitiveComparator.GreaterThanOrEqual(
                                new SemanticVersion(major, minor, 0, Operand._preReleases, null)),
                            PrimitiveComparator.LessThan(
                                new SemanticVersion(major, minor + 1, 0, SemanticPreRelease.ZeroArray, null))
                        ); // TODO: check for overflow
                    }
                    return (
                        PrimitiveComparator.GreaterThanOrEqual(
                            new SemanticVersion(major, 0, 0, Operand._preReleases, null)),
                        PrimitiveComparator.LessThan(
                            new SemanticVersion(major + 1, 0, 0, SemanticPreRelease.ZeroArray, null))
                    ); // TODO: check for overflow

                default:
                    throw new NotImplementedException();
            }
        }

        public static XRangeComparator GreaterThan(PartialVersion operand)
            => new XRangeComparator(operand, PrimitiveOperator.GreaterThan);
        public static XRangeComparator GreaterThanOrEqual(PartialVersion operand)
            => new XRangeComparator(operand, PrimitiveOperator.GreaterThanOrEqual);
        public static XRangeComparator LessThan(PartialVersion operand)
            => new XRangeComparator(operand, PrimitiveOperator.LessThan);
        public static XRangeComparator LessThanOrEqual(PartialVersion operand)
            => new XRangeComparator(operand, PrimitiveOperator.LessThanOrEqual);
        public static XRangeComparator Equal(PartialVersion operand)
            => new XRangeComparator(operand, PrimitiveOperator.Equal);

    }
}
