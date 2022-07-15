using System.Collections.Generic;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        public static IEnumerable<object?[]> EnumerateFormatFixtures()
        {
            List<FormatFixture> fixtures = new List<FormatFixture>();
            FormatFixture New(string input, string? format = null)
            {
                FormatFixture fixture = new FormatFixture(input, format);
                fixtures.Add(fixture);
                return fixture;
            }



            // Default format
            New("1.2.3");
            New("12.345.6789");
            New("123.45.6-alpha.6");
            New("1.2.3+BUILD.007");
            New("1.2.3-alpha.0+DEV.00");

            // simple format
            New("1.2.3", "M.m.p").Returns("1.2.3");
            New("1.2.3-pre+build", "M.m.p-rr+dd").Returns("1.2.3-pre+build");

            // identifiers
            New("1.2.3", "M.m.p-rr+dd").Returns("1.2.3");
            New("1.2.3-pre+build", "M.m.p").Returns("1.2.3");
            New("1.2.3-pre+build", "M.m.p+dd").Returns("1.2.3+build");

            // weird orders
            New("1.2.3-pre.2+build.00", "dd+rr-p.m.M").Returns("build.00+pre.2-3.2.1");

            // optional components
            New("1.2.3-beta", "M.m.pp-rr").Returns("1.2.3-beta");
            New("1.2.0-beta", "M.m.pp-rr").Returns("1.2-beta");
            New("1.2.0-beta", "M.mm.p-rr").Returns("1.2.0-beta");
            New("1.0.0-beta", "M.mm.p-rr").Returns("1.0-beta");
            New("1.2.3-beta", "M.mm.pp-rr").Returns("1.2.3-beta");
            New("1.2.0-beta", "M.mm.pp-rr").Returns("1.2-beta");
            New("1.0.0-beta", "M.mm.pp-rr").Returns("1-beta");
            New("1.0.3-beta", "M.mm.pp-rr").Returns("1.0.3-beta");

            // Escaped sequences
            New("1.2.3-alpha", @"M.m.p-rr \M.\m.\p-\r\r").Returns("1.2.3-alpha M.m.p-rr");
            New("1.2.3-alpha", "M.m.p-rr 'M.m.p-rr'").Returns("1.2.3-alpha M.m.p-rr");
            New("1.2.3-alpha", "M.m.p-rr \"M.m.p-rr\"").Returns("1.2.3-alpha M.m.p-rr");
            New("1.2.3-alpha", "'M.m.p-rr: M.m.p-rr").Throws("The format string contains an unclosed quote ('\\'', '\"').");
            New("1.2.3-alpha", "\"M.m.p-rr: M.m.p-rr").Throws("The format string contains an unclosed quote ('\\'', '\"').");

            // indexed identifiers
            New("1.2.3", "-r0.r1.r2").Returns("");
            New("1.2.3-beta", "-r0.r1.r2").Returns("-beta");
            New("1.2.3-alpha.3.beta.5.gamma", "-r0.r1.r2").Returns("-alpha.3.beta");
            New("1.2.3-alpha.3.beta.5.gamma", "-r2.r").Returns("-beta.5");
            New("1.2.3-alpha.3.beta.5.gamma", "-r2.rr").Returns("-beta.5.gamma");
            New("1.2.3-alpha.3.beta.5.gamma", "-r4.r").Returns("-gamma");
            New("1.2.3-alpha.3.beta.5.gamma", "-r4.rr").Returns("-gamma");
            New("1.2.3-alpha.3.beta.5.gamma", "-r4.r0.rr").Returns("-gamma.alpha.3.beta.5.gamma");

            New("1.2.3", "+d0.d1.d2").Returns("");
            New("1.2.3+build", "+d0.d1.d2").Returns("+build");
            New("1.2.3+build.007.dev.2.test", "+d0.d1.d2").Returns("+build.007.dev");
            New("1.2.3+build.007.dev.2.test", "+d2.d").Returns("+dev.2");
            New("1.2.3+build.007.dev.2.test", "+d2.dd").Returns("+dev.2.test");
            New("1.2.3+build.007.dev.2.test", "+d4.d").Returns("+test");
            New("1.2.3+build.007.dev.2.test", "+d4.dd").Returns("+test");
            New("1.2.3+build.007.dev.2.test", "+d4.d0.dd").Returns("+test.build.007.dev.2.test");



            return TestUtil.CreateFixtures(fixtures.ToArray());
        }

    }
}
