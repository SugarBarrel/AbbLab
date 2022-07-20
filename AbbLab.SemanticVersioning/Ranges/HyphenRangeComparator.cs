using System;
using System.ComponentModel;
using static AbbLab.SemanticVersioning.PrimitiveComparator;

namespace AbbLab.SemanticVersioning
{
    public sealed class HyphenRangeComparator : AdvancedComparator
    {
        public HyphenRangeComparator(PartialVersion from, PartialVersion to) : base(from) => To = to;
        public PartialVersion From => base.Operand;
        public PartialVersion To { get; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete($"You should use the {nameof(From)} property instead, when you know it's an instance of the {nameof(HyphenRangeComparator)} class.")]
        public new PartialVersion Operand => base.Operand;

        protected override (PrimitiveComparator?, PrimitiveComparator?) ConvertToPrimitives()
        {
            (PrimitiveComparator?, PrimitiveComparator?) tuple = (ConvertFrom(From), ConvertTo(To));
            if (tuple.Item1 is null && tuple.Item2 is not null)
            {
                tuple.Item1 = tuple.Item2;
                tuple.Item2 = null;
            }
            return tuple;

            static PrimitiveComparator? ConvertFrom(PartialVersion from)
            {
                // x.x.x - ... ⇒ * ...
                if (!from.Major.IsNumeric) return null;

                // 1.x.x - ... ⇒ >=1.0.0 ...
                if (!from.Minor.IsNumeric)
                    return GreaterThanOrEqual(new SemanticVersion(from.Major.Number, 0, 0, null, null));

                // 1.2.x - ... ⇒ >=1.2.0 ...
                if (!from.Patch.IsNumeric)
                    return GreaterThanOrEqual(new SemanticVersion(from.Major.Number, from.Minor.Number, 0, null, null));

                // 1.2.3 - ... ⇒ >=1.2.3 ...
                return GreaterThanOrEqual(new SemanticVersion(from));
            }
            static PrimitiveComparator? ConvertTo(PartialVersion to)
            {
                // ... - x.x.x ⇒ ... *
                if (!to.Major.IsNumeric) return null;

                // ... - 1.x.x ⇒ ... <2.0.0-0
                if (!to.Minor.IsNumeric)
                    return LessThan(new SemanticVersion(to.Major.Number + 1, 0, 0, SemanticPreRelease.ZeroArray, null));

                // ... - 1.2.x ⇒ ... <1.3.0-0
                if (!to.Patch.IsNumeric)
                    return LessThan(new SemanticVersion(to.Major.Number, to.Minor.Number + 1, 0, SemanticPreRelease.ZeroArray, null));

                // ... - 1.2.3 ⇒ ... <=1.2.3
                return LessThanOrEqual(new SemanticVersion(to));
            }
        }

    }
}
