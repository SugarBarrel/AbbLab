namespace AbbLab.SemanticVersioning
{
    public enum IncrementType : byte
    {
        None = 0,

        Major,
        Minor,
        Patch,
        PreMajor,
        PreMinor,
        PrePatch,

        PreRelease,
    }
}
