using System;
using System.Text;
using Xunit;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        [Theory, MemberData(nameof(EnumerateFormatFixtures))]
        public void FormattingTests(FormatFixture fixture)
        {
            Output.WriteLine($"Formatting v{fixture.Version}");
            Output.WriteLine($"Using {(fixture.Format is not null ? $"`{fixture.Format}`" : "default format")}");
            Output.WriteLine($"Expected: {(fixture.Expected is not null ? $"`{fixture.Expected}`" : "format exception")}");

            SemanticVersion version = SemanticVersion.Parse(fixture.Version);

            if (fixture.Format is null)
            {
                AssertEx.Identical(new Func<string>[]
                {
                    () => version.ToString(),
                    () => version.ToString(null),
                    () => version.ToString("G"),
                    () => version.ToString("g"),
                    () => ((IFormattable)version).ToString(null, null),
                    () => ((IFormattable)version).ToString("G", null),
                    () => ((IFormattable)version).ToString("g", null),
                }, fixture.Assert);
            }
            else
            {
                AssertEx.Identical(new Func<string>[]
                {
                    () => version.ToString(fixture.Format),
                    () => ((IFormattable)version).ToString(fixture.Format, null),
                }, fixture.Assert);
            }
        }

        public class FormatFixture
        {
            public FormatFixture(string version, string? format = null)
            {
                Version = version;
                Format = format;
                if (format is null) Expected = version;
            }
            public string Version { get; }
            public string? Format { get; }
            public string? Expected { get; set; }

            public bool IsValid => ErrorType is null;
            public Type? ErrorType { get; private set; }
            public string? ErrorMessage { get; private set; }

            public FormatFixture Returns(string expected)
            {
                Expected = expected;
                return this;
            }
            public FormatFixture Throws(string message) => Throws<FormatException>(message);
            public FormatFixture Throws<TException>(string message)
            {
                ErrorType = typeof(TException);
                ErrorMessage = message;
                return this;
            }

            public void Assert(string? result, Exception? exception)
            {
                if (IsValid)
                {
                    Xunit.Assert.NotNull(result);
                    Xunit.Assert.Equal(Expected, result);
                }
                else
                {
                    Xunit.Assert.NotNull(exception);
                    Xunit.Assert.Equal(ErrorType, exception!.GetType());
                    Xunit.Assert.StartsWith(ErrorMessage!, exception.Message);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('v').Append(Version);
                if (Format is not null) sb.Append(" {").Append(Format).Append('}');
                return sb.ToString();
            }

        }
    }
}
