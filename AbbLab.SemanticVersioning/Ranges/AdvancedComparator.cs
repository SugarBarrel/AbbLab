namespace AbbLab.SemanticVersioning
{
    public abstract class AdvancedComparator : Comparator
    {
        public override bool IsPrimitive => false;
        public abstract (PrimitiveComparator, PrimitiveComparator?) ToPrimitives();
    }
}