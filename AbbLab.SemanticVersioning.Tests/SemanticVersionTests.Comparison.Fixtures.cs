namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        public static readonly string?[] ComparisonFixtures =
        {
            null,
            "0.0.0-0",
            "0.0.0-alpha",
            "0.0.0",
            "0.0.1-0",
            "0.0.1",
            "0.0.9",
            "0.1.0",
            "0.2.0-pre",
            "0.2.0-pre.0",
            "0.2.0-pre.1",
            "0.2.0-pre.alpha",
            "0.2.0-pre.alpha-beta",
            "1.0.0-0",
            "1.0.0-beta",
            "1.0.0-beta.0",
            "1.0.0",
            "1.1.0",
            "1.2147483647.0",
            "2147483647.1.2-0",
            "2147483647.1.2",
            "2147483647.2147483647.2147483647-0",
            "2147483647.2147483647.2147483647",
        };

    }
}
