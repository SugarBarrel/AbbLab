namespace AbbLab.SemanticVersioning
{
    public abstract class AdvancedComparator : Comparator
    {
        public abstract (PrimitiveComparator, PrimitiveComparator?) ToPrimitives();
    }
}