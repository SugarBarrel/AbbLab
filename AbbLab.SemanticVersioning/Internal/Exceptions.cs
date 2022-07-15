using System;
using JetBrains.Annotations;

namespace AbbLab.SemanticVersioning
{
    internal static class Exceptions
    {
        public const string MajorNotFound = "The major version component could not be found.";
        public const string MinorNotFound = "The minor version component could not be found.";
        public const string PatchNotFound = "The patch version component could not be found.";
        public const string PreReleaseNotFound = "The pre-release identifier could not be found.";
        public const string BuildMetadataNotFound = "The build metadata identifier could not be found.";

        public const string ComponentLeadingZeroes = "The version component cannot contain leading zeroes.";
        public const string MajorLeadingZeroes = "The major version component cannot contain leading zeroes.";
        public const string MinorLeadingZeroes = "The minor version component cannot contain leading zeroes.";
        public const string PatchLeadingZeroes = "The patch version component cannot contain leading zeroes.";
        public const string PreReleaseLeadingZeroes = "The numeric pre-release identifier cannot contain leading zeroes.";

        public const string ComponentTooBig = "The version component cannot be greater than 2147483647.";
        public const string MajorTooBig = "The major version component cannot be greater than 2147483647.";
        public const string MinorTooBig = "The minor version component cannot be greater than 2147483647.";
        public const string PatchTooBig = "The patch version component cannot be greater than 2147483647.";
        public const string PreReleaseTooBig = "The numeric pre-release identifier cannot be greater than 2147483647.";

        public const string ComponentNegative = "The version component cannot be less than 0.";
        public const string MajorNegative = "The major version component cannot be less than 0.";
        public const string MinorNegative = "The minor version component cannot be less than 0.";
        public const string PatchNegative = "The patch version component cannot be less than 0.";
        public const string PreReleaseNegative = "The numeric pre-release identifier cannot be less than 0.";

        public const string PreReleaseEmpty = "The pre-release identifier cannot be empty.";
        public const string BuildMetadataEmpty = "The build metadata identifier cannot be empty.";

        public const string ComponentInvalid = "The version component must be either numeric or a wildcard character.";
        public const string PreReleaseInvalid = "The pre-release identifier must only contain [A-Za-z0-9-] characters.";
        public const string BuildMetadataInvalid = "The build metadata identifier must only contain [A-Za-z0-9-] characters.";

        public const string ComponentNotNumeric = "The version component is not numeric.";
        public const string ComponentNotWildcard = "The version component is not a wildcard.";
        public const string PreReleaseNotAlphanumeric = "The pre-release identifier is not alphanumeric.";
        public const string PreReleaseNotNumeric = "The pre-release identifier is not numeric.";

        public const string Leftovers = "Encountered an invalid character after the parsed version.";

        [Pure] public static string GetMessage(this SemanticErrorCode code) => code switch
        {
            SemanticErrorCode.MajorNotFound => MajorNotFound,
            SemanticErrorCode.MinorNotFound => MinorNotFound,
            SemanticErrorCode.PatchNotFound => PatchNotFound,
            SemanticErrorCode.PreReleaseNotFound => PreReleaseNotFound,
            SemanticErrorCode.BuildMetadataNotFound => BuildMetadataNotFound,

            SemanticErrorCode.MajorLeadingZeroes => MajorLeadingZeroes,
            SemanticErrorCode.MinorLeadingZeroes => MinorLeadingZeroes,
            SemanticErrorCode.PatchLeadingZeroes => PatchLeadingZeroes,
            SemanticErrorCode.PreReleaseLeadingZeroes => PreReleaseLeadingZeroes,
            SemanticErrorCode.ComponentLeadingZeroes => ComponentLeadingZeroes,

            SemanticErrorCode.MajorTooBig => MajorTooBig,
            SemanticErrorCode.MinorTooBig => MinorTooBig,
            SemanticErrorCode.PatchTooBig => PatchTooBig,
            SemanticErrorCode.PreReleaseTooBig => PreReleaseTooBig,
            SemanticErrorCode.ComponentTooBig => ComponentTooBig,

            SemanticErrorCode.PreReleaseEmpty => PreReleaseEmpty,
            SemanticErrorCode.BuildMetadataEmpty => BuildMetadataEmpty,

            SemanticErrorCode.PreReleaseInvalid => PreReleaseInvalid,
            SemanticErrorCode.BuildMetadataInvalid => BuildMetadataInvalid,
            SemanticErrorCode.ComponentInvalid => ComponentInvalid,

            SemanticErrorCode.LEFTOVERS => Leftovers,

            _ => throw new ArgumentException($"{code} error code is not supposed to have a message."),
        };

    }
}
