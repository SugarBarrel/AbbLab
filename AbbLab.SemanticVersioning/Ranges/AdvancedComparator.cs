namespace AbbLab.SemanticVersioning
{
    public abstract class AdvancedComparator : Comparator
    {
        public PartialVersion Operand { get; }
        private bool primitivesSet;
        private PrimitiveComparator? primitiveLeft;
        private PrimitiveComparator? primitiveRight;

        protected AdvancedComparator(PartialVersion operand) => Operand = operand;

        public sealed override bool IsPrimitive => false;

        public sealed override bool HasPreReleaseVersion(int major, int minor, int patch)
            => Operand.IsPreRelease
               && (Operand.Major.IsWildcard || Operand.Major.GetValueOrZero() == major)
               && (Operand.Minor.IsWildcard || Operand.Minor.GetValueOrZero() == minor)
               && (Operand.Patch.IsWildcard || Operand.Patch.GetValueOrZero() == patch);

        public sealed override bool Satisfies(SemanticVersion version)
        {
            (PrimitiveComparator? left, PrimitiveComparator? right) = ToPrimitives();
            return left?.Satisfies(version) is not false && right?.Satisfies(version) is not false;
        }

        public (PrimitiveComparator?, PrimitiveComparator?) ToPrimitives()
        {
            if (!primitivesSet)
            {
                (primitiveLeft, primitiveRight) = ConvertToPrimitives();
                primitivesSet = true;
            }
            return (primitiveLeft, primitiveRight);
        }
        protected abstract (PrimitiveComparator?, PrimitiveComparator?) ConvertToPrimitives();

    }
}