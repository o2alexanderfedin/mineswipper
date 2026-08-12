namespace Interviews.RottingOranges;

/// <summary>
/// Builds starting fields for demonstrations and tests.
/// </summary>
public static class GridFactory
{
    /// <summary>
    /// Returns a field of fresh oranges with <paramref name="rottenCount"/> of them replaced by
    /// rotten ones and <paramref name="holeCount"/> replaced by holes, all at distinct random
    /// positions.
    /// </summary>
    /// <param name="rows">The number of rows. Must be positive.</param>
    /// <param name="columns">The number of columns. Must be positive.</param>
    /// <param name="rottenCount">How many oranges start out rotten.</param>
    /// <param name="holeCount">
    /// How many cells are empty. Rot cannot pass through a hole, so enough of them will strand
    /// oranges the rot can never reach.
    /// </param>
    /// <param name="random">
    /// The source of randomness. Seed it to reproduce a particular field.
    /// </param>
    /// <returns>A grid of <see cref="CellState"/> values encoded as integers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A dimension is not positive, a count is negative, or the counts together do not fit
    /// the field.
    /// </exception>
    public static int[][] CreateRandomField(
        int rows,
        int columns,
        int rottenCount,
        int holeCount,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(rottenCount);
        ArgumentOutOfRangeException.ThrowIfNegative(holeCount);

        int cellCount = rows * columns;

        if (rottenCount + (long)holeCount > cellCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(holeCount),
                holeCount,
                $"A {rows}x{columns} field holds {cellCount} cells, which cannot fit " +
                $"{rottenCount} rotten oranges and {holeCount} holes.");
        }

        int[][] grid = new int[rows][];

        for (int row = 0; row < rows; row++)
        {
            grid[row] = new int[columns];
            Array.Fill(grid[row], (int)CellState.Fresh);
        }

        // Partial Fisher-Yates over the flattened cell indices: draws distinct positions
        // without ever retrying a collision, so rotten oranges and holes never overlap.
        int[] positions = new int[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            positions[i] = i;
        }

        int drawn = 0;

        for (int i = 0; i < rottenCount; i++)
        {
            Place(grid, positions, drawn++, cellCount, columns, random, CellState.Rotten);
        }

        for (int i = 0; i < holeCount; i++)
        {
            Place(grid, positions, drawn++, cellCount, columns, random, CellState.Empty);
        }

        return grid;
    }

    /// <summary>
    /// Draws the next unused position and writes <paramref name="state"/> into it.
    /// </summary>
    private static void Place(
        int[][] grid,
        int[] positions,
        int drawn,
        int cellCount,
        int columns,
        Random random,
        CellState state)
    {
        int pick = random.Next(drawn, cellCount);
        (positions[drawn], positions[pick]) = (positions[pick], positions[drawn]);

        int chosen = positions[drawn];
        grid[chosen / columns][chosen % columns] = (int)state;
    }
}
