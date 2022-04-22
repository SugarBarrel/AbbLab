using System;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace AbbLab.SemanticVersioning
{
    [Flags]
    internal enum SemanticErrorCode : byte
    {
        Success = 0,

        IdentifierMask  = 0b_0000_1111,

        MAJOR           = 0b_0000_0001,
        MINOR           = 0b_0000_0010,
        PATCH           = 0b_0000_0011,
        PRERELEASE      = 0b_0000_0100,
        BUILD_METADATA  = 0b_0000_0101,

        NatureMask         = 0b_1111_0000,

        NOT_FOUND          = 0b_0001_0000,
        TOO_BIG            = 0b_0010_0000,
        LEADING_ZEROES     = 0b_0011_0000,
        EMPTY              = 0b_0100_0000,
        INVALID            = 0b_0101_0000,
        LEFTOVERS          = 0b_0110_0000,

        MajorNotFound         = MAJOR          | NOT_FOUND,
        MinorNotFound         = MINOR          | NOT_FOUND,
        PatchNotFound         = PATCH          | NOT_FOUND,
        PreReleaseNotFound    = PRERELEASE     | NOT_FOUND,
        BuildMetadataNotFound = BUILD_METADATA | NOT_FOUND,

        MajorTooBig      = MAJOR      | TOO_BIG,
        MinorTooBig      = MINOR      | TOO_BIG,
        PatchTooBig      = PATCH      | TOO_BIG,
        PreReleaseTooBig = PRERELEASE | TOO_BIG,

        MajorLeadingZeroes      = MAJOR      | LEADING_ZEROES,
        MinorLeadingZeroes      = MINOR      | LEADING_ZEROES,
        PatchLeadingZeroes      = PATCH      | LEADING_ZEROES,
        PreReleaseLeadingZeroes = PRERELEASE | LEADING_ZEROES,

        PreReleaseEmpty    = PRERELEASE     | EMPTY,
        BuildMetadataEmpty = BUILD_METADATA | EMPTY,

        PreReleaseInvalid    = PRERELEASE     | INVALID,
        BuildMetadataInvalid = BUILD_METADATA | INVALID,

    }
}
