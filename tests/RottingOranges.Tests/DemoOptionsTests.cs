using Interviews.RottingOranges.Demo;

namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="DemoOptions"/>, which reads the demo's
/// <c>[seed [pattern]]</c> command line.
/// </summary>
public sealed class DemoOptionsTests
{
    [Fact]
    public void TryParse_NoArguments_UsesARandomFieldAndOrthogonalSpread()
    {
        Assert.True(DemoOptions.TryParse([], out DemoOptions? options, out string? error));

        Assert.Null(error);
        Assert.Equal(new DemoOptions(Seed: null, SpreadPattern.Orthogonal), options);
    }

    [Fact]
    public void TryParse_SeedOnly_KeepsOrthogonalSpread()
    {
        Assert.True(DemoOptions.TryParse(["2024"], out DemoOptions? options, out _));

        Assert.Equal(new DemoOptions(2024, SpreadPattern.Orthogonal), options);
    }

    [Theory]
    [InlineData("diagonal", SpreadPattern.Diagonal)]
    [InlineData("Diagonal", SpreadPattern.Diagonal)]
    [InlineData("ORTHOGONAL", SpreadPattern.Orthogonal)]
    public void TryParse_SeedAndPatternName_ReadsBoth(string pattern, SpreadPattern expected)
    {
        Assert.True(DemoOptions.TryParse(["2024", pattern], out DemoOptions? options, out _));

        Assert.Equal(new DemoOptions(2024, expected), options);
    }

    [Theory]
    [InlineData("7")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("orthogonal,diagonal")]
    public void TryParse_PatternThatIsNotAName_IsRejected(string pattern)
    {
        // Enum parsing would take these as numbers or flag combinations; "7" is no pattern at
        // all and used to reach the solver, which crashed the demo with a stack trace.
        Assert.False(DemoOptions.TryParse(["2024", pattern], out DemoOptions? options, out string? error));

        Assert.Null(options);
        Assert.Contains(pattern, error);
        Assert.Contains("orthogonal", error);
        Assert.Contains("diagonal", error);
    }

    [Fact]
    public void TryParse_MisspelledPattern_IsRejectedRatherThanIgnored()
    {
        Assert.False(DemoOptions.TryParse(["2024", "diagnal"], out _, out string? error));

        Assert.Contains("diagnal", error);
    }

    [Theory]
    [InlineData("diagonal")]
    [InlineData("abc")]
    [InlineData("12.5")]
    public void TryParse_FirstArgumentThatIsNotASeed_IsRejectedRatherThanIgnored(string seed)
    {
        // A pattern in the seed's place used to be dropped without a word, leaving a random
        // field with orthogonal spread.
        Assert.False(DemoOptions.TryParse([seed], out DemoOptions? options, out string? error));

        Assert.Null(options);
        Assert.Contains(seed, error);
    }

    [Fact]
    public void TryParse_ExtraArguments_AreRejected()
    {
        Assert.False(DemoOptions.TryParse(["2024", "diagonal", "fast"], out _, out string? error));

        Assert.Contains("fast", error);
    }

    [Fact]
    public void Usage_NamesEveryPattern()
    {
        foreach (string name in Enum.GetNames<SpreadPattern>())
        {
            Assert.Contains(name.ToLowerInvariant(), DemoOptions.Usage);
        }
    }
}
