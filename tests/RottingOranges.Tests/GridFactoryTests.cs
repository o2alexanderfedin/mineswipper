namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="GridFactory"/>.
/// </summary>
public sealed class GridFactoryTests
{
    [Fact]
    public void CreateRandomField_HasTheRequestedShape()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, new Random(42));

        Assert.Equal(10, field.Length);
        Assert.All(field, row => Assert.Equal(20, row.Length));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(200)]
    public void CreateRandomField_PlacesExactlyTheRequestedNumberOfRottenOranges(int rottenCount)
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, rottenCount, new Random(42));

        int rotten = field.SelectMany(row => row).Count(cell => cell == (int)CellState.Rotten);
        int fresh = field.SelectMany(row => row).Count(cell => cell == (int)CellState.Fresh);

        Assert.Equal(rottenCount, rotten);
        Assert.Equal((10 * 20) - rottenCount, fresh);
    }

    [Fact]
    public void CreateRandomField_IsReproducibleForAGivenSeed()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 3, new Random(2024));
        int[][] second = GridFactory.CreateRandomField(10, 20, 3, new Random(2024));

        Assert.Equal(first, second);
    }

    [Fact]
    public void CreateRandomField_VariesBetweenSeeds()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 1, new Random(1));
        int[][] second = GridFactory.CreateRandomField(10, 20, 1, new Random(2));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task CreateRandomField_ProducesAFieldTheSolverCanConsume()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, new Random(99));

        int minutes = await RottingOrangesSolver.OrangesRottingAsync(field);

        // A single source on an otherwise fresh field always reaches every orange, and never
        // needs more minutes than the furthest corner is away.
        Assert.InRange(minutes, 1, (10 - 1) + (20 - 1));
    }

    [Fact]
    public void CreateRandomField_ThrowsOnNullRandom()
    {
        Assert.Throws<ArgumentNullException>(() => GridFactory.CreateRandomField(10, 20, 1, null!));
    }

    [Theory]
    [InlineData(0, 20, 1)]
    [InlineData(10, 0, 1)]
    [InlineData(10, 20, -1)]
    [InlineData(10, 20, 201)]
    public void CreateRandomField_ThrowsOnInvalidArguments(int rows, int columns, int rottenCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GridFactory.CreateRandomField(rows, columns, rottenCount, new Random(42)));
    }
}
