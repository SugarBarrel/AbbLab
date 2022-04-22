using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AbbLab.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionParsing
    {
        private readonly ITestOutputHelper Output;
        public SemanticVersionParsing(ITestOutputHelper output) => Output = output;

        [Theory, MemberData(nameof(EnumerateFixtures))]
        public void Test(ParseFixture fixture)
        {
            Output.WriteLine($"`{fixture.Input}`");
            Output.WriteLine($"using {fixture.Options}");

            AssertEx.Identical(new Func<SemanticVersion>[]
            {
                () => SemanticVersion.Parse(fixture.Input, fixture.Options),
                () => SemanticVersion.Parse(fixture.Input.AsSpan(), fixture.Options),
            }, fixture.Assert);
            AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
            {
                (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input, fixture.Options, out res),
                (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input.AsSpan(), fixture.Options, out res),
            }, fixture.Assert);

            if (fixture.Options is SemanticOptions.Strict) // test strict overloads and loose parsing with strict options
            {
                AssertEx.Identical(new Func<SemanticVersion>[]
                {
                    () => SemanticVersion.Parse(fixture.Input),
                    () => SemanticVersion.Parse(fixture.Input.AsSpan()),
                    () => SemanticVersion.Parse(fixture.Input, TestUtil.PseudoStrict),
                    () => SemanticVersion.Parse(fixture.Input.AsSpan(), TestUtil.PseudoStrict),
                }, fixture.Assert);

                AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
                {
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input.AsSpan(), out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input, TestUtil.PseudoStrict, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input.AsSpan(), TestUtil.PseudoStrict, out res),
                }, fixture.Assert);
            }

            if (fixture.IsValid) // if the version is valid, loose parsing should return the same result
            {
                AssertEx.Identical(new Func<SemanticVersion>[]
                {
                    () => SemanticVersion.Parse(fixture.Input, SemanticOptions.Loose),
                    () => SemanticVersion.Parse(fixture.Input.AsSpan(), SemanticOptions.Loose),
                }, fixture.Assert);

                AssertEx.Identical(new AssertEx.TryParse<SemanticVersion>[]
                {
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input, SemanticOptions.Loose, out res),
                    (out SemanticVersion? res) => SemanticVersion.TryParse(fixture.Input.AsSpan(), SemanticOptions.Loose, out res),
                }, fixture.Assert);
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

                int index = Input.IndexOf('<'); // Syntax: "1.2.3-pre<$>.beta"
                if (index is -1 || Input.IndexOf('>', index) != index + 2) return this;
                ErrorPosition = index;
                Input = Input[..index] + Input[index + 1] + Input[(index + 3)..];

                return this;
            }

            public void Assert(SemanticVersion? version)
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
                    Xunit.Assert.Null(version);
                    throw (Exception)ErrorType!.GetConstructor(new Type[1] { typeof(string) })!.Invoke(new object?[] { ErrorMessage });
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
