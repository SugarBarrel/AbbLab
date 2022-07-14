using System;
using Xunit;

namespace AbbLab.SemanticVersioning.Tests
{
    public partial class SemanticVersionTests
    {
        [Fact]
        public void ComparisonTest()
        {
            int length = ComparisonFixtures.Length;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    static SemanticVersion? Parse(string? str) => str is null ? null : SemanticVersion.Parse(str);

                    SemanticVersion? a = Parse(ComparisonFixtures[i]);
                    SemanticVersion? b = Parse(ComparisonFixtures[j]);

                    if (a is not null)
                    {
                        Assert.Equal(i.Equals(j), a.Equals(b));
                        Assert.Equal(i.Equals(j), a.Equals((object?)b));

                        Assert.Equal(Math.Sign(i.CompareTo(j)), Math.Sign(a.CompareTo(b)));
                        Assert.Equal(Math.Sign(((IComparable)i).CompareTo(j)), Math.Sign(((IComparable)a).CompareTo(b)));
                        Assert.Throws<ArgumentException>(() => ((IComparable)a).CompareTo("not a version"));

                        if (i == j)
                        {
                            Assert.True(a.Equals(a));
                            Assert.Equal(0, a.CompareTo(a));
                            Assert.Equal(a.GetHashCode(), b!.GetHashCode());
                        }
                    }

                    Assert.Equal(i == j, a == b);
                    Assert.Equal(i != j, a != b);

                    Assert.Equal(i > j, a > b);
                    Assert.Equal(i < j, a < b);
                    Assert.Equal(i >= j, a >= b);
                    Assert.Equal(i <= j, a <= b);
                }
        }

    }
}
