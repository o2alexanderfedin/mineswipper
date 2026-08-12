namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Specification for <see cref="SpreadPattern"/>: how the choice of neighbourhood changes
/// where the rot can reach and how fast it gets there.
/// </summary>
public sealed class SpreadPatternTests
{
    [Fact]
    public async Task Orthogonal_IsTheDefault()
    {
        int[][] grid = [[2, 1, 1], [1, 1, 0], [0, 1, 1]];

        Assert.Equal(
            await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Orthogonal),
            await RottingOrangesSolver.OrangesRottingAsync(grid));
    }

    [Fact]
    public async Task Diagonal_CrossesAGapOrthogonalSpreadCannot()
    {
        // The lone orange touches the source only at a corner; the two cells between them
        // are empty, so orthogonal rot has nowhere to go.
        int[][] grid = [[2, 0], [0, 1]];

        Assert.Equal(-1, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Orthogonal));
        Assert.Equal(1, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Diagonal));
    }

    [Fact]
    public async Task Diagonal_ReachesTheCornersOfASquare()
    {
        // Every one of the eight cells around the centre rots in the first minute.
        int[][] grid = [[1, 1, 1], [1, 2, 1], [1, 1, 1]];

        Assert.Equal(2, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Orthogonal));
        Assert.Equal(1, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Diagonal));
    }

    [Fact]
    public async Task Diagonal_HalvesTheClassicExample()
    {
        int[][] grid = [[2, 1, 1], [1, 1, 0], [0, 1, 1]];

        Assert.Equal(4, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Orthogonal));
        Assert.Equal(2, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Diagonal));
    }

    [Fact]
    public async Task Diagonal_StillReportsMinusOneWhenHolesEncloseAnOrange()
    {
        // All eight neighbours are empty, so no pattern can reach the centre orange.
        int[][] grid =
        [
            [2, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 1, 0],
            [0, 0, 0, 0],
        ];

        Assert.Equal(-1, await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Diagonal));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(99)]
    [InlineData(2024)]
    public async Task Diagonal_IsNeverSlowerAndNeverReachesLess(int seed)
    {
        int[][] field = GridFactory.CreateRandomField(
            rows: 12,
            columns: 24,
            rottenCount: 1,
            holeCells: (12 * 24) / 4,
            holeSize: 10,
            new Random(seed));

        int orthogonal = await RottingOrangesSolver.OrangesRottingAsync(field, pattern: SpreadPattern.Orthogonal);
        int diagonal = await RottingOrangesSolver.OrangesRottingAsync(field, pattern: SpreadPattern.Diagonal);

        // Every orthogonal step is also a diagonal step, so whatever the rot could reach
        // before it can still reach - at least as quickly.
        if (orthogonal >= 0)
        {
            Assert.True(
                diagonal >= 0 && diagonal <= orthogonal,
                $"orthogonal took {orthogonal} minutes but diagonal reported {diagonal}");
        }
    }

    [Fact]
    public async Task Diagonal_TracesTheSameWayAsOrthogonal()
    {
        await using StringWriter writer = new();

        int minutes = await RottingOrangesSolver.OrangesRottingAsync(
            [[2, 0], [0, 1]],
            writer,
            SpreadPattern.Diagonal);

        Assert.Equal(1, minutes);
        Assert.Contains("Minute 0 (initial) - 1 fresh orange left:", writer.ToString(), StringComparison.Ordinal);
        Assert.Contains("Minute 1 - 0 fresh oranges left:", writer.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public async Task UndefinedPattern_Throws(int pattern)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => RottingOrangesSolver.OrangesRottingAsync([[2, 1]], pattern: (SpreadPattern)pattern));
    }

    [Fact]
    public async Task Diagonal_MatchesOrthogonalWhereNoDiagonalMoveExists()
    {
        // A single row has no diagonal neighbours in bounds, so the extra offsets can make no
        // difference. This pins the containment the comparison test above relies on: the
        // diagonal pattern adds moves, it never replaces the orthogonal ones.
        int[][] singleRow = [[2, 1, 1, 1]];

        Assert.Equal(
            await RottingOrangesSolver.OrangesRottingAsync(singleRow, pattern: SpreadPattern.Orthogonal),
            await RottingOrangesSolver.OrangesRottingAsync(singleRow, pattern: SpreadPattern.Diagonal));
    }
}
