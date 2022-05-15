using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AbbLab.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        private readonly ITestOutputHelper Output;
        public SemanticVersionTests(ITestOutputHelper output) => Output = output;

        [Theory, MemberData(nameof(EnumerateParseFixtures))]
        public void ParsingTests(ParseFixture fixture)
        {
            string input = fixture.Input;
            SemanticOptions options = fixture.Options;
            Output.WriteLine($"`{input}`");
            Output.WriteLine($"using {options}");

            AssertEx.Identical(new Func<SemanticVersion>[]
            {
                () => SemanticVersion.Parse(input, options),
                () => SemanticVersion.Parse(input.AsSpan(), options),
            }, fixture.Assert);
            AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
            {
                (out SemanticVersion? res) => SemanticVersion.TryParse(input, options, out res),
                (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), options, out res),
                (out SemanticVersion? res) => SemanticVersion.TryParse(input, options, out _, out res),
                (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), options, out _, out res),
            }, fixture.Assert);

            if (fixture.Options is SemanticOptions.Strict) // test strict overloads and loose parsing with strict options
            {
                AssertEx.Identical(new Func<SemanticVersion>[]
                {
                    () => SemanticVersion.Parse(input),
                    () => SemanticVersion.Parse(input.AsSpan()),
                    () => SemanticVersion.Parse(input, TestUtil.PseudoStrict),
                    () => SemanticVersion.Parse(input.AsSpan(), TestUtil.PseudoStrict),
                }, fixture.Assert);
                AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
                {
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input, TestUtil.PseudoStrict, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), TestUtil.PseudoStrict, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input, TestUtil.PseudoStrict, out _, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), TestUtil.PseudoStrict, out _, out res),
                }, fixture.Assert);
            }

            if (fixture.IsValid) // if the version is valid, loose parsing should return the same result
            {
                AssertEx.Identical(new Func<SemanticVersion>[]
                {
                    () => SemanticVersion.Parse(input, SemanticOptions.Loose),
                    () => SemanticVersion.Parse(input.AsSpan(), SemanticOptions.Loose),
                }, fixture.Assert);
                AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
                {
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input, SemanticOptions.Loose, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), SemanticOptions.Loose, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input, SemanticOptions.Loose, out _, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(input.AsSpan(), SemanticOptions.Loose, out _, out res),
                }, fixture.Assert);
            }

            if (fixture.ErrorPosition is not null)
            {
                SemanticVersion.TryParse(input, options, out int lastPosition, out _);
                SemanticVersion.TryParse(input.AsSpan(), options, out int lastPosition2, out _);
                Assert.Equal(fixture.ErrorPosition.Value, lastPosition);
                Assert.Equal(fixture.ErrorPosition.Value, lastPosition2);
            }
        }

        public class ParseFixture
        {
            public string Input { get; private set; }
            public SemanticOptions Options { get; }

            public int? Major { get; private set; }
            public int? Minor { get; private set; }
            public int? Patch { get; private set; }
            public ReadOnlyCollection<object>? PreReleases { get; private set; }
            public ReadOnlyCollection<string>? BuildMetadata { get; private set; }

            public Type? ErrorType { get; private set; }
            public string? ErrorMessage { get; private set; }
            public int? ErrorPosition { get; private set; }

            public bool IsValid => ErrorType is null;

            public ParseFixture(string input, SemanticOptions options = SemanticOptions.Strict)
            {
                Input = input;
                Options = options;
            }

            public ParseFixture Returns(int major, int minor, int patch, params object[] identifiers)
            {
                Major = major;
                Minor = minor;
                Patch = patch;

                if (!Array.TrueForAll(identifiers, static i => i is string or int))
                    throw new ArgumentException("One of the identifiers is not a string or an integer.", nameof(identifiers));

                int buildMetadataStart = Array.FindIndex(identifiers, static i => i is string str && str.StartsWith('+'));
                if (buildMetadataStart is -1)
                {
                    PreReleases = new ReadOnlyCollection<object>(identifiers);
                    BuildMetadata = ReadOnlyCollection.Empty<string>();
                }
                else
                {
                    PreReleases = new ReadOnlyCollection<object>(identifiers[..buildMetadataStart]);
                    identifiers[buildMetadataStart] = ((string)identifiers[buildMetadataStart])[1..]; // remove leading '+'
                    BuildMetadata = new ReadOnlyCollection<string>(identifiers[buildMetadataStart..].Cast<string>());
                }
                return this;
            }
            public ParseFixture Throws(string message) => Throws<ArgumentException>(message);
            public ParseFixture Throws<TException>(string message)
            {
                ErrorType = typeof(TException);
                ErrorMessage = message;

                int index = Input.IndexOf('{'); // Syntax: "1.2.3-pre<$>.beta"
                if (index is -1 || Input.IndexOf('}', index) != index + 2) return this;
                ErrorPosition = index;
                Input = Input[..index] + Input[index + 1] + Input[(index + 3)..];

                return this;
            }

            public void Assert(SemanticVersion? version, Exception? exception)
            {
                if (IsValid)
                {
                    Xunit.Assert.NotNull(version);
                    Xunit.Assert.Equal(Major, version!.Major);
                    Xunit.Assert.Equal(Minor, version.Minor);
                    Xunit.Assert.Equal(Patch, version.Patch);
                    Xunit.Assert.Equal(PreReleases, version.PreReleases.Select(static p => p.IsNumeric ? (object)p.Number : p.Text));
                    Xunit.Assert.Equal(BuildMetadata, version.BuildMetadata);
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
                sb.Append('"').Append(Input).Append('"');
                if (Options is not SemanticOptions.Strict)
                    sb.Append(", ").Append(Options);
                return sb.ToString();
            }

        }
    }
}
