using System;

namespace AbbLab.SemanticVersioning
{
    [Flags]
    public enum SemanticOptions
    {
        /// <summary>
        ///   <para>The strict semantic version parsing mode, as per the SemVer 2.0.0 specification.</para>
        /// </summary>
        Strict = 0,

        /// <summary>
        ///   <para>Allows <c>'v'</c> or <c>'V'</c> as a prefix to the version. Example: <c>v2.7.5</c>.</para>
        /// </summary>
        AllowVersionPrefix          = 1 << 0,
        /// <summary>
        ///   <para>Allows <c>'='</c> as a prefix to the version. Example: <c>=3.0.0</c>.</para>
        /// </summary>
        AllowEqualsPrefix           = 1 << 1,
        /// <summary>
        ///   <para>Allows leading whitespace characters in the string to parse.</para>
        /// </summary>
        AllowLeadingWhite           = 1 << 2,
        /// <summary>
        ///   <para>Allows trailing whitespace characters in the string to parse.</para>
        /// </summary>
        AllowTrailingWhite          = 1 << 3,
        /// <summary>
        ///   <para>Allows whitespaces between version components and identifiers. Example: <c>= v 1 .4. 6- alpha</c>.</para>
        /// </summary>
        AllowInnerWhite             = 1 << 4,
        /// <summary>
        ///   <para>Allows leading zeroes in version components and numeric pre-release identifiers. Example: <c>1.02.5-alpha.007</c>.</para>
        /// </summary>
        AllowLeadingZeroes          = 1 << 5,
        /// <summary>
        ///   <para>Allows omitting both minor and patch version components. Example: <c>1-beta.4</c>.</para>
        /// </summary>
        OptionalMinor               = 1 << 6,
        /// <summary>
        ///   <para>Allows omitting the patch version component. Example: <c>1.2-nightly.456</c>.</para>
        /// </summary>
        OptionalPatch               = 1 << 7,
        /// <summary>
        ///   <para>Allows specifying pre-release identifiers in an old-style fashion. Example: <c>4.0.0alpha2beta7</c> is resolved as having <c>alpha</c>, <c>2</c>, <c>beta</c> and <c>7</c> pre-releases.</para>
        /// </summary>
        OptionalPreReleaseSeparator = 1 << 8,
        /// <summary>
        ///   <para>Allows passing a string that contains non-version characters at the end.</para>
        /// </summary>
        AllowLeftovers              = 1 << 9,
        RemoveEmptyPreReleases      = 1 << 10,
        RemoveEmptyBuildMetadata    = 1 << 11,

        /// <summary>
        ///   <para>Specifies all parsing options to be able to parse normally invalid versions.</para>
        /// </summary>
        Loose = ~0,
    }
}
