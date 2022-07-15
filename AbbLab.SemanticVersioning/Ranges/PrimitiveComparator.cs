using System;

namespace AbbLab.SemanticVersioning
{
    public sealed class PrimitiveComparator : Comparator
    {
        private PrimitiveComparator(SemanticVersion operand, PrimitiveOperator type)
        {
            Operand = operand;
            Type = type;
        }

        public SemanticVersion Operand { get; }
        public PrimitiveOperator Type { get; }
        public override bool IsPrimitive => true;

        public override bool Satisfies(SemanticVersion version) => Type switch
        {
            PrimitiveOperator.GreaterThan => version > Operand,
            PrimitiveOperator.GreaterThanOrEqual => version >= Operand,
            PrimitiveOperator.LessThan => version < Operand,
            PrimitiveOperator.LessThanOrEqual => version <= Operand,
            PrimitiveOperator.Equal => version == Operand,
            _ => throw new NotImplementedException(),
        };

        public static PrimitiveComparator Create(SemanticVersion operand, PrimitiveOperator @operator)
        {
            if (@operator is not PrimitiveOperator.GreaterThan and not PrimitiveOperator.GreaterThanOrEqual
                and not PrimitiveOperator.LessThan and not PrimitiveOperator.LessThanOrEqual and not PrimitiveOperator.Equal)
                throw new ArgumentException($"{@operator} is not a valid primitive operator.", nameof(@operator));
            return new PrimitiveComparator(operand, @operator);
        }

        public static PrimitiveComparator GreaterThan(SemanticVersion operand)
            => new PrimitiveComparator(operand, PrimitiveOperator.GreaterThan);
        public static PrimitiveComparator GreaterThanOrEqual(SemanticVersion operand)
            => new PrimitiveComparator(operand, PrimitiveOperator.GreaterThanOrEqual);
        public static PrimitiveComparator LessThan(SemanticVersion operand)
            => new PrimitiveComparator(operand, PrimitiveOperator.LessThan);
        public static PrimitiveComparator LessThanOrEqual(SemanticVersion operand)
            => new PrimitiveComparator(operand, PrimitiveOperator.LessThanOrEqual);
        public static PrimitiveComparator Equal(SemanticVersion operand)
            => new PrimitiveComparator(operand, PrimitiveOperator.Equal);

    }
}