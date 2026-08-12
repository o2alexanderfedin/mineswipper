namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="GridFactory"/>.
/// </summary>
public sealed class GridFactoryTests
{
    private static int Count(int[][] field, CellState state) =>
        field.SelectMany(row => row).Count(cell => cell == (int)state);

    [Fact]
    public void CreateRandomField_HasTheRequestedShape()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, 24, new Random(42));

        Assert.Equal(10, field.Length);
        Assert.All(field, row => Assert.Equal(20, row.Length));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1, 24)]
    [InlineData(7, 50)]
    [InlineData(100, 100)]
    public void CreateRandomField_PlacesExactlyTheRequestedCounts(int rottenCount, int holeCount)
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, rottenCount, holeCount, new Random(42));

        Assert.Equal(rottenCount, Count(field, CellState.Rotten));
        Assert.Equal(holeCount, Count(field, CellState.Empty));
        Assert.Equal((10 * 20) - rottenCount - holeCount, Count(field, CellState.Fresh));
    }

    [Fact]
    public void CreateRandomField_NeverPutsAHoleWhereARottenOrangeGoes()
    {
        // Fill the field completely: any overlap would show up as a missing cell of one kind.
        int[][] field = GridFactory.CreateRandomField(10, 20, 80, 120, new Random(7));

        Assert.Equal(80, Count(field, CellState.Rotten));
        Assert.Equal(120, Count(field, CellState.Empty));
        Assert.Equal(0, Count(field, CellState.Fresh));
    }

    [Fact]
    public void CreateRandomField_ScattersHolesRatherThanClusteringThemInOneRow()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, 40, new Random(2024));

        int rowsWithHoles = field.Count(row => row.Contains((int)CellState.Empty));

        Assert.True(rowsWithHoles > 1, $"holes landed in only {rowsWithHoles} row(s)");
    }

    [Fact]
    public void CreateRandomField_IsReproducibleForAGivenSeed()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 3, 24, new Random(2024));
        int[][] second = GridFactory.CreateRandomField(10, 20, 3, 24, new Random(2024));

        Assert.Equal(first, second);
    }

    [Fact]
    public void CreateRandomField_VariesBetweenSeeds()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 1, 24, new Random(1));
        int[][] second = GridFactory.CreateRandomField(10, 20, 1, 24, new Random(2));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task CreateRandomField_ProducesAFieldTheSolverCanConsume()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, 24, new Random(99));

        int minutes = await RottingOrangesSolver.OrangesRottingAsync(field);

        // Holes can strand oranges, so -1 is a legitimate outcome; what must never happen is
        // a minute count beyond what the field could possibly need.
        Assert.InRange(minutes, -1, (10 * 20) - 1);
    }

    [Fact]
    public async Task CreateRandomField_WithEnoughHolesStrandsOranges()
    {
        // A hole either side of the source seals it off from the rest of the row.
        int[][] field = GridFactory.CreateRandomField(1, 5, 0, 5, new Random(3));
        field[0][2] = (int)CellState.Rotten;
        field[0][0] = (int)CellState.Fresh;

        Assert.Equal(-1, await RottingOrangesSolver.OrangesRottingAsync(field));
    }

    [Fact]
    public void CreateRandomField_ThrowsOnNullRandom()
    {
        Assert.Throws<ArgumentNullException>(
            () => GridFactory.CreateRandomField(10, 20, 1, 24, null!));
    }

    [Theory]
    [InlineData(0, 20, 1, 0)]
    [InlineData(10, 0, 1, 0)]
    [InlineData(10, 20, -1, 0)]
    [InlineData(10, 20, 0, -1)]
    [InlineData(10, 20, 201, 0)]
    [InlineData(10, 20, 0, 201)]
    [InlineData(10, 20, 100, 101)]
    public void CreateRandomField_ThrowsOnInvalidArguments(
        int rows,
        int columns,
        int rottenCount,
        int holeCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GridFactory.CreateRandomField(rows, columns, rottenCount, holeCount, new Random(42)));
    }
}
