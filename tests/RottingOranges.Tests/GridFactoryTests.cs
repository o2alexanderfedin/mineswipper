namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Behavioural specification for <see cref="GridFactory"/>.
/// </summary>
public sealed class GridFactoryTests
{
    private static int Count(int[][] field, CellState state) =>
        field.SelectMany(row => row).Count(cell => cell == (int)state);

    /// <summary>
    /// Returns the size of every orthogonally connected run of empty cells, largest first.
    /// </summary>
    private static List<int> HoleRegionSizes(int[][] field)
    {
        (int Row, int Column)[] offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];
        bool[][] seen = field.Select(row => new bool[row.Length]).ToArray();
        List<int> sizes = [];

        for (int row = 0; row < field.Length; row++)
        {
            for (int column = 0; column < field[row].Length; column++)
            {
                if (seen[row][column] || field[row][column] != (int)CellState.Empty)
                {
                    continue;
                }

                int size = 0;
                Queue<(int Row, int Column)> queue = new();
                queue.Enqueue((row, column));
                seen[row][column] = true;

                while (queue.Count > 0)
                {
                    (int currentRow, int currentColumn) = queue.Dequeue();
                    size++;

                    foreach ((int rowOffset, int columnOffset) in offsets)
                    {
                        int nextRow = currentRow + rowOffset;
                        int nextColumn = currentColumn + columnOffset;

                        if (nextRow < 0 || nextRow >= field.Length ||
                            nextColumn < 0 || nextColumn >= field[nextRow].Length ||
                            seen[nextRow][nextColumn] ||
                            field[nextRow][nextColumn] != (int)CellState.Empty)
                        {
                            continue;
                        }

                        seen[nextRow][nextColumn] = true;
                        queue.Enqueue((nextRow, nextColumn));
                    }
                }

                sizes.Add(size);
            }
        }

        sizes.Sort((left, right) => right.CompareTo(left));
        return sizes;
    }

    [Fact]
    public void CreateRandomField_HasTheRequestedShape()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, 24, 6, new Random(42));

        Assert.Equal(10, field.Length);
        Assert.All(field, row => Assert.Equal(20, row.Length));
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 24, 1)]
    [InlineData(1, 24, 8)]
    [InlineData(7, 50, 12)]
    [InlineData(7, 50, 200)]
    [InlineData(100, 100, 25)]
    public void CreateRandomField_CarvesExactlyTheRequestedArea(
        int rottenCount,
        int holeCells,
        int holeSize)
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, rottenCount, holeCells, holeSize, new Random(42));

        Assert.Equal(rottenCount, Count(field, CellState.Rotten));
        Assert.Equal(holeCells, Count(field, CellState.Empty));
        Assert.Equal((10 * 20) - rottenCount - holeCells, Count(field, CellState.Fresh));
    }

    [Fact]
    public void CreateRandomField_GrowsOneHoleAsASingleConnectedBlob()
    {
        // The whole carved area belongs to one hole, so it must come out in one piece.
        int[][] field = GridFactory.CreateRandomField(10, 20, 0, 30, 30, new Random(2024));

        Assert.Equal([30], HoleRegionSizes(field));
    }

    [Fact]
    public void CreateRandomField_LargerHoleSizeProducesLargerHoles()
    {
        const int holeCells = 60;

        int[][] scattered = GridFactory.CreateRandomField(10, 20, 0, holeCells, 1, new Random(7));
        int[][] clumped = GridFactory.CreateRandomField(10, 20, 0, holeCells, 20, new Random(7));

        Assert.Equal(holeCells, Count(scattered, CellState.Empty));
        Assert.Equal(holeCells, Count(clumped, CellState.Empty));

        // Same carved area, but clumped into far fewer, far larger holes.
        Assert.True(
            HoleRegionSizes(clumped)[0] >= 20,
            $"largest clumped hole was only {HoleRegionSizes(clumped)[0]} cells");
        Assert.True(
            HoleRegionSizes(clumped).Count < HoleRegionSizes(scattered).Count,
            "clumping should leave fewer distinct holes than scattering");
    }

    [Fact]
    public void CreateRandomField_HoleSizeOfOneScattersSingleCells()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 0, 10, 1, new Random(11));

        // Single-cell holes may still touch by chance, but must not form a large blob.
        Assert.Equal(10, Count(field, CellState.Empty));
        Assert.True(HoleRegionSizes(field)[0] <= 3, "scattered holes clumped unexpectedly");
    }

    [Fact]
    public void CreateRandomField_NeverCarvesAHoleOverARottenOrange()
    {
        // Fill the field completely: any overlap would show up as a missing cell of one kind.
        int[][] field = GridFactory.CreateRandomField(10, 20, 80, 120, 15, new Random(7));

        Assert.Equal(80, Count(field, CellState.Rotten));
        Assert.Equal(120, Count(field, CellState.Empty));
        Assert.Equal(0, Count(field, CellState.Fresh));
    }

    [Fact]
    public void CreateRandomField_MakesUpTheShortfallWhenAHoleIsBoxedIn()
    {
        // Rotten oranges take nearly every cell, so holes must be assembled from the scraps
        // left between them rather than grown as one blob.
        int[][] field = GridFactory.CreateRandomField(10, 20, 190, 10, 50, new Random(5));

        Assert.Equal(190, Count(field, CellState.Rotten));
        Assert.Equal(10, Count(field, CellState.Empty));
    }

    [Fact]
    public void CreateRandomField_IsReproducibleForAGivenSeed()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 3, 24, 8, new Random(2024));
        int[][] second = GridFactory.CreateRandomField(10, 20, 3, 24, 8, new Random(2024));

        Assert.Equal(first, second);
    }

    [Fact]
    public void CreateRandomField_VariesBetweenSeeds()
    {
        int[][] first = GridFactory.CreateRandomField(10, 20, 1, 24, 8, new Random(1));
        int[][] second = GridFactory.CreateRandomField(10, 20, 1, 24, 8, new Random(2));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task CreateRandomField_ProducesAFieldTheSolverCanConsume()
    {
        int[][] field = GridFactory.CreateRandomField(10, 20, 1, 24, 8, new Random(99));

        int minutes = await RottingOrangesSolver.OrangesRottingAsync(field);

        // Holes can strand oranges, so -1 is a legitimate outcome; what must never happen is
        // a minute count beyond what the field could possibly need.
        Assert.InRange(minutes, -1, (10 * 20) - 1);
    }

    [Fact]
    public async Task CreateRandomField_WithEnoughHolesStrandsOranges()
    {
        // A hole either side of the source seals it off from the rest of the row.
        int[][] field = GridFactory.CreateRandomField(1, 5, 0, 5, 1, new Random(3));
        field[0][2] = (int)CellState.Rotten;
        field[0][0] = (int)CellState.Fresh;

        Assert.Equal(-1, await RottingOrangesSolver.OrangesRottingAsync(field));
    }

    [Fact]
    public void CreateRandomField_ThrowsOnNullRandom()
    {
        Assert.Throws<ArgumentNullException>(
            () => GridFactory.CreateRandomField(10, 20, 1, 24, 8, null!));
    }

    [Theory]
    [InlineData(0, 20, 1, 0, 1)]
    [InlineData(10, 0, 1, 0, 1)]
    [InlineData(10, 20, -1, 0, 1)]
    [InlineData(10, 20, 0, -1, 1)]
    [InlineData(10, 20, 0, 10, 0)]
    [InlineData(10, 20, 0, 10, -1)]
    [InlineData(10, 20, 201, 0, 1)]
    [InlineData(10, 20, 0, 201, 1)]
    [InlineData(10, 20, 100, 101, 1)]
    public void CreateRandomField_ThrowsOnInvalidArguments(
        int rows,
        int columns,
        int rottenCount,
        int holeCells,
        int holeSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GridFactory.CreateRandomField(rows, columns, rottenCount, holeCells, holeSize, new Random(42)));
    }
}
