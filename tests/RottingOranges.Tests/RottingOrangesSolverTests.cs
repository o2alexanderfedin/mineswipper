namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for
/// <see cref="RottingOrangesSolver.OrangesRottingAsync(int[][], IWaveObserver, CancellationToken)"/>.
/// </summary>
public sealed class RottingOrangesSolverTests
{
    public static TheoryData<string, int[][], int> Grids => new()
    {
        // Name, grid, expected minutes.
        {
            "LeetCode example 1 - spreads to every orange",
            [[2, 1, 1], [1, 1, 0], [0, 1, 1]],
            4
        },
        {
            "LeetCode example 2 - bottom-left orange is unreachable",
            [[2, 1, 1], [0, 1, 1], [1, 0, 1]],
            -1
        },
        {
            "LeetCode example 3 - no fresh oranges at all",
            [[0, 2]],
            0
        },
        {
            "Empty cells only",
            [[0, 0, 0]],
            0
        },
        {
            "Single rotten orange",
            [[2]],
            0
        },
        {
            "Single fresh orange with no source of rot",
            [[1]],
            -1
        },
        {
            "Every orange already rotten",
            [[2, 2], [2, 2]],
            0
        },
        {
            "Fresh orange walled off by empty cells",
            [[2, 0, 1]],
            -1
        },
        {
            "Two sources rot the grid from opposite corners",
            [[2, 1, 1], [1, 1, 1], [1, 1, 2]],
            2
        },
        {
            "Non-square grid - rot travels the long way round",
            [[2, 1, 1, 1, 1]],
            4
        },
        {
            "Tall single-column grid",
            [[1], [1], [2], [1]],
            2
        },
        {
            "Rot must detour around a wall",
            [[2, 0, 1], [1, 0, 1], [1, 1, 1]],
            6
        },
    };

    [Theory]
    [MemberData(nameof(Grids))]
    public async Task OrangesRotting_ReturnsExpectedMinutes(string scenario, int[][] grid, int expected)
    {
        int actual = await RottingOrangesSolver.OrangesRottingAsync(grid);

        Assert.Equal(expected, actual);
        Assert.False(string.IsNullOrWhiteSpace(scenario));
    }

    [Fact]
    public async Task OrangesRotting_DoesNotMutateCallerGrid()
    {
        int[][] grid = [[2, 1, 1], [1, 1, 0], [0, 1, 1]];

        _ = await RottingOrangesSolver.OrangesRottingAsync(grid);

        Assert.Equal([2, 1, 1], grid[0]);
        Assert.Equal([1, 1, 0], grid[1]);
        Assert.Equal([0, 1, 1], grid[2]);
    }

    [Fact]
    public async Task OrangesRotting_HandlesLargeGridWithinLinearTime()
    {
        const int size = 500;
        int[][] grid = new int[size][];
        for (int r = 0; r < size; r++)
        {
            grid[r] = new int[size];
            Array.Fill(grid[r], (int)CellState.Fresh);
        }

        grid[0][0] = (int)CellState.Rotten;

        // Rot spreads by Manhattan distance from the single corner source.
        Assert.Equal((size - 1) * 2, await RottingOrangesSolver.OrangesRottingAsync(grid));
    }

    [Fact]
    public async Task OrangesRotting_ThrowsOnNullGrid()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => RottingOrangesSolver.OrangesRottingAsync(null!));
    }

    [Fact]
    public async Task OrangesRotting_ThrowsOnNullRow()
    {
        int[][] grid = [[0, 1], null!];

        await Assert.ThrowsAsync<ArgumentException>(() => RottingOrangesSolver.OrangesRottingAsync(grid));
    }

    [Fact]
    public async Task OrangesRotting_ThrowsOnEmptyGrid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => RottingOrangesSolver.OrangesRottingAsync([]));
    }

    [Fact]
    public async Task OrangesRotting_ThrowsOnEmptyRow()
    {
        int[][] grid = [[]];

        await Assert.ThrowsAsync<ArgumentException>(() => RottingOrangesSolver.OrangesRottingAsync(grid));
    }

    [Fact]
    public async Task OrangesRotting_ThrowsOnRaggedGrid()
    {
        int[][] grid = [[1, 2], [1]];

        await Assert.ThrowsAsync<ArgumentException>(() => RottingOrangesSolver.OrangesRottingAsync(grid));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task OrangesRotting_ThrowsOnUnknownCellValue(int value)
    {
        int[][] grid = [[(int)CellState.Rotten, value]];

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => RottingOrangesSolver.OrangesRottingAsync(grid));
    }
}
