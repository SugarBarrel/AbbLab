using System.Collections.Generic;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        public static IEnumerable<object?[]> EnumerateParseFixtures()
        {
            List<ParseFixture> fixtures = new List<ParseFixture>();
            ParseFixture New(string input, SemanticOptions options = SemanticOptions.Strict)
            {
                ParseFixture fixture = new ParseFixture(input, options);
                fixtures.Add(fixture);
                return fixture;
            }

            // Notes:
            // - numeric pre-release identifiers must be represented by `int`;
            // - the first identifier starting with a '+' indicates the beginning of build metadata identifiers;
            // - enclosing a character in '{' '}' in the input string indicates where the exception should be thrown;



            // simple
            New("1.2.3").Returns(1, 2, 3);
            // with zeroes
            New("0.0.1").Returns(0, 0, 1);
            New("0.1.0").Returns(0, 1, 0);
            New("1.0.0").Returns(1, 0, 0);
            // more digits
            New("1.234.56789").Returns(1, 234, 56789);
            New("12345.678.9").Returns(12345, 678, 9);

            // big numbers
            New("2147483647.2.3").Returns(2147483647, 2, 3);
            New("2147483648.2.3").Throws(Exceptions.MajorTooBig);
            New("1.2147483647.3").Returns(1, 2147483647, 3);
            New("1.2147483648.3").Throws(Exceptions.MinorTooBig);
            New("1.2.2147483647").Returns(1, 2, 2147483647);
            New("1.2.2147483648").Throws(Exceptions.PatchTooBig);
            New("2147483647.2147483647.2147483647").Returns(2147483647, 2147483647, 2147483647);

            // with pre-releases
            New("1.2.3-alpha").Returns(1, 2, 3, "alpha");
            New("1.2.3-456").Returns(1, 2, 3, 456);
            New("1.2.3-alpha7-beta").Returns(1, 2, 3, "alpha7-beta");
            New("1.2.3-beta.6.alpha").Returns(1, 2, 3, "beta", 6, "alpha");
            // leading and trailing hyphens
            New("1.2.3--45.beta-").Returns(1, 2, 3, "-45", "beta-");
            New("1.2.3---beta---.45-").Returns(1, 2, 3, "--beta---", "45-");

            // with build metadata
            New("1.2.3+test-build").Returns(1, 2, 3, "+test-build");
            New("1.2.3+045.test").Returns(1, 2, 3, "+045", "test");
            New("1.2.3+-045.test").Returns(1, 2, 3, "+-045", "test");
            // with pre-releases and build metadata
            New("1.2.3-alpha.7+build.008").Returns(1, 2, 3, "alpha", 7, "+build", "008");

            // leading zeroes
            SemanticOptions o = SemanticOptions.AllowLeadingZeroes;
            New("001.2.3").Throws(Exceptions.MajorLeadingZeroes);
            New("1.002.3").Throws(Exceptions.MinorLeadingZeroes);
            New("1.2.003").Throws(Exceptions.PatchLeadingZeroes);
            New("1.2.3-004").Throws(Exceptions.PreReleaseLeadingZeroes);
            New("1.2.3-004a").Returns(1, 2, 3, "004a");
            New("1.2.3+007").Returns(1, 2, 3, "+007");
            New("001.2.3", o).Returns(1, 2, 3);
            New("1.002.3", o).Returns(1, 2, 3);
            New("1.2.003", o).Returns(1, 2, 3);
            New("1.2.3-004", o).Returns(1, 2, 3, 4);
            New("1.2.3-004a", o).Returns(1, 2, 3, "004a");
            New("1.2.3+007", o).Returns(1, 2, 3, "+007");

            // equals prefix
            o = SemanticOptions.AllowEqualsPrefix;
            New("{=}1.2.3").Throws(Exceptions.MajorNotFound);
            New("=1.2.3", o).Returns(1, 2, 3);
            New("={=}1.2.3", o).Throws(Exceptions.MajorNotFound); // only one '=' is allowed
            // version prefix
            o = SemanticOptions.AllowVersionPrefix;
            New("{v}1.2.3").Throws(Exceptions.MajorNotFound);
            New("v1.2.3", o).Returns(1, 2, 3);
            New("V1.2.3", o).Returns(1, 2, 3);
            New("v{v}1.2.3", o).Throws(Exceptions.MajorNotFound); // only one 'v'/'V' is allowed
            New("v{V}1.2.3", o).Throws(Exceptions.MajorNotFound);
            // equals and version prefixes
            o = SemanticOptions.AllowEqualsPrefix | SemanticOptions.AllowVersionPrefix;
            New("{=}v1.2.3").Throws(Exceptions.MajorNotFound);
            New("=v1.2.3", o).Returns(1, 2, 3);
            New("=V1.2.3", o).Returns(1, 2, 3);
            New("v{=}1.2.3", o).Throws(Exceptions.MajorNotFound); // only in this order: '=' ('v' | 'V')

            // optional patch
            o = SemanticOptions.OptionalPatch;
            New("1.2").Throws(Exceptions.PatchNotFound);
            New("1.2.").Throws(Exceptions.PatchNotFound);
            New("1.2", o).Returns(1, 2, 0);
            New("1.2.", o).Returns(1, 2, 0);
            // optional minor
            o = SemanticOptions.OptionalMinor;
            New("1").Throws(Exceptions.MinorNotFound);
            New("1.").Throws(Exceptions.MinorNotFound);
            New("1", o).Returns(1, 0, 0);
            New("1.", o).Returns(1, 0, 0);

            // optional pre-release separator
            o = SemanticOptions.OptionalPreReleaseSeparator;
            New("1.2.3alpha").Throws(Exceptions.Leftovers);
            New("1.2.3alpha5b70").Throws(Exceptions.Leftovers);
            New("1.2.3alpha", o).Returns(1, 2, 3, "alpha");
            New("1.2.3alpha5b70", o).Returns(1, 2, 3, "alpha", 5, "b", 70);
            // TODO: add tests with '.', '-'

            // remove empty pre-releases
            o = SemanticOptions.RemoveEmptyPreReleases;
            New("1.2.3-.alpha..").Throws(Exceptions.PreReleaseNotFound);
            New("1.2.3-..4.").Throws(Exceptions.PreReleaseNotFound);
            New("1.2.3-.alpha..", o).Returns(1, 2, 3, "alpha");
            New("1.2.3-..4.", o).Returns(1, 2, 3, 4);
            // remove empty build metadata
            o = SemanticOptions.RemoveEmptyBuildMetadata;
            New("1.2.3+.test-build..").Throws(Exceptions.BuildMetadataNotFound);
            New("1.2.3+..007.").Throws(Exceptions.BuildMetadataNotFound);
            New("1.2.3+.test-build..", o).Returns(1, 2, 3, "+test-build");
            New("1.2.3+..007.", o).Returns(1, 2, 3, "+007");

            // leading whitespace
            New(" \r\t\n 1.2.3").Throws(Exceptions.MajorNotFound);
            o = SemanticOptions.AllowLeadingWhite;
            New(" \r\t\n 1.2.3", o).Returns(1, 2, 3);

            // trailing whitespace
            New("1.2.3-pre+build \r\t\n ").Throws(Exceptions.Leftovers);
            o = SemanticOptions.AllowTrailingWhite;
            New("1.2.3-pre+build \r\t\n ", o).Returns(1, 2, 3, "pre", "+build");

            // allow leftovers
            New("1.2.3-gamma+123$$$").Throws(Exceptions.Leftovers);
            o = SemanticOptions.AllowLeftovers;
            New("1.2.3-gamma+123$$$", o).Returns(1, 2, 3, "gamma", "+123");

            // inner whitespace
            New("1 .\r2\t\n. 3\r\t-\nalpha .\r\t0\n+ build").Throws(Exceptions.MinorNotFound);
            o = SemanticOptions.AllowInnerWhite;
            New("1 .\r2\t\n. 3\r\t-\nalpha .\r\t0\n+ build", o).Returns(1, 2, 3, "alpha", 0, "+build");

            // inner whitespace + trailing whitespace
            o = SemanticOptions.AllowInnerWhite;
            New("1 .2 .3 -beta +007   ", o).Throws(Exceptions.Leftovers);
            New("1 .2 .3 -beta +007   $$$", o).Throws(Exceptions.Leftovers);
            o = SemanticOptions.AllowInnerWhite | SemanticOptions.AllowTrailingWhite;
            New("1 .2 .3 -beta +007   ", o).Returns(1, 2, 3, "beta", "+007");
            New("1 .2 .3 -beta +007   $$$", o).Throws(Exceptions.Leftovers);



            return TestUtil.CreateFixtures(fixtures.ToArray());
        }

    }
}
