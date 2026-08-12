namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="GridFormatter"/>.
/// </summary>
public sealed class GridFormatterTests
{
    [Theory]
    [InlineData(CellState.Empty, GridFormatter.EmptySymbol)]
    [InlineData(CellState.Fresh, GridFormatter.FreshSymbol)]
    [InlineData(CellState.Rotten, GridFormatter.RottenSymbol)]
    public void ToSymbol_MapsEveryDefinedState(CellState state, string expected)
    {
        Assert.Equal(expected, GridFormatter.ToSymbol(state));
    }

    [Fact]
    public void ToSymbol_ThrowsOnUndefinedState()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GridFormatter.ToSymbol((CellState)42));
    }

    [Fact]
    public void Format_RendersOneLinePerRowWithoutTrailingNewline()
    {
        CellState[][] cells =
        [
            [CellState.Rotten, CellState.Fresh, CellState.Empty],
            [CellState.Empty, CellState.Fresh, CellState.Fresh],
        ];

        string expected = string.Join(
            Environment.NewLine,
            "\U0001f7e4\U0001f34a⬛",
            "⬛\U0001f34a\U0001f34a");

        Assert.Equal(expected, GridFormatter.Format(cells));
    }

    [Fact]
    public void Format_ThrowsOnNullGrid()
    {
        Assert.Throws<ArgumentNullException>(() => GridFormatter.Format(null!));
    }

    [Fact]
    public void Legend_NamesEverySymbol()
    {
        string legend = GridFormatter.Legend;

        Assert.Contains(GridFormatter.EmptySymbol, legend, StringComparison.Ordinal);
        Assert.Contains(GridFormatter.FreshSymbol, legend, StringComparison.Ordinal);
        Assert.Contains(GridFormatter.RottenSymbol, legend, StringComparison.Ordinal);
    }
}
