using Interviews.RottingOranges.Demo;

namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="ConsoleField"/>, which sizes the demo's field to
/// the console window.
/// </summary>
public sealed class ConsoleFieldTests
{
    [Fact]
    public void Compute_FillsTheWindowWithoutScrolling()
    {
        (int rows, int columns) = ConsoleField.Compute(windowWidth: 80, windowHeight: 24, reservedLines: 7);

        Assert.Equal(24 - 7, rows);
        Assert.Equal(39, columns);
    }

    [Theory]
    [InlineData(80, 39)]
    [InlineData(100, 49)]
    [InlineData(201, 100)]
    public void Compute_FitsDoubleWidthCellsWithAColumnToSpare(int windowWidth, int expectedColumns)
    {
        (_, int columns) = ConsoleField.Compute(windowWidth, windowHeight: 50, reservedLines: 0);

        Assert.Equal(expectedColumns, columns);

        // The rendered row must leave at least one column free, or it wraps and every row
        // below it lands in the wrong place.
        Assert.True(
            (columns * ConsoleField.CellWidth) < windowWidth,
            $"{columns} cells need {columns * ConsoleField.CellWidth} columns of {windowWidth}");
    }

    [Fact]
    public void Compute_LeavesRoomForTheHeadingsAndSummary()
    {
        const int windowHeight = 40;
        const int reserved = 7;

        (int rows, _) = ConsoleField.Compute(windowWidth: 120, windowHeight: windowHeight, reservedLines: reserved);

        Assert.Equal(windowHeight - reserved, rows);
        Assert.True(rows + reserved <= windowHeight);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void Compute_FallsBackWhenTheWindowCannotBeMeasured(int windowWidth, int windowHeight)
    {
        (int rows, int columns) = ConsoleField.Compute(windowWidth, windowHeight, reservedLines: 7);

        (int fallbackRows, int fallbackColumns) = ConsoleField.Compute(
            ConsoleField.FallbackWidth,
            ConsoleField.FallbackHeight,
            reservedLines: 7);

        Assert.Equal(fallbackRows, rows);
        Assert.Equal(fallbackColumns, columns);
    }

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(80, 24, 100)]
    [InlineData(4, 5, 3)]
    public void Compute_NeverReturnsAFieldTooSmallToPlay(int windowWidth, int windowHeight, int reservedLines)
    {
        (int rows, int columns) = ConsoleField.Compute(windowWidth, windowHeight, reservedLines);

        Assert.True(rows >= ConsoleField.MinimumSize, $"rows was {rows}");
        Assert.True(columns >= ConsoleField.MinimumSize, $"columns was {columns}");
    }

    [Fact]
    public void Compute_ThrowsOnNegativeReservedLines()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConsoleField.Compute(80, 24, reservedLines: -1));
    }

    [Fact]
    public void Measure_ReturnsTheFallbackSizeWhenOutputIsRedirected()
    {
        // The test host always runs with output redirected, so this exercises that branch.
        Assert.True(Console.IsOutputRedirected);

        (int rows, int columns) = ConsoleField.Measure(reservedLines: 7);
        (int expectedRows, int expectedColumns) = ConsoleField.Compute(
            ConsoleField.FallbackWidth,
            ConsoleField.FallbackHeight,
            reservedLines: 7);

        Assert.Equal(expectedRows, rows);
        Assert.Equal(expectedColumns, columns);
    }

    [Fact]
    public async Task Compute_ProducesAFieldTheSolverCanConsume()
    {
        (int rows, int columns) = ConsoleField.Compute(120, 40, reservedLines: 7);

        int[][] field = GridFactory.CreateRandomField(
            rows,
            columns,
            rottenCount: 1,
            holeCells: (rows * columns) / 4,
            holeSize: 10,
            new Random(2024));

        int minutes = await RottingOrangesSolver.OrangesRottingAsync(field);

        Assert.InRange(minutes, -1, rows * columns);
    }
}
