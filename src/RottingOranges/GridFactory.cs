namespace Interviews.RottingOranges;

/// <summary>
/// Builds starting fields for demonstrations and tests.
/// </summary>
public static class GridFactory
{
    /// <summary>
    /// Returns a field of fresh oranges with <paramref name="rottenCount"/> of them replaced by
    /// rotten ones at distinct random positions.
    /// </summary>
    /// <param name="rows">The number of rows. Must be positive.</param>
    /// <param name="columns">The number of columns. Must be positive.</param>
    /// <param name="rottenCount">
    /// How many oranges start out rotten. Must be between zero and <c>rows * columns</c>.
    /// </param>
    /// <param name="random">
    /// The source of randomness. Seed it to reproduce a particular field.
    /// </param>
    /// <returns>A grid of <see cref="CellState"/> values encoded as integers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A dimension is not positive, or <paramref name="rottenCount"/> does not fit the field.
    /// </exception>
    public static int[][] CreateRandomField(int rows, int columns, int rottenCount, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(rottenCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rottenCount, rows * columns);

        int[][] grid = new int[rows][];

        for (int row = 0; row < rows; row++)
        {
            grid[row] = new int[columns];
            Array.Fill(grid[row], (int)CellState.Fresh);
        }

        // Partial Fisher-Yates over the flattened cell indices: draws distinct positions
        // without ever retrying a collision.
        int cellCount = rows * columns;
        int[] positions = new int[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            positions[i] = i;
        }

        for (int i = 0; i < rottenCount; i++)
        {
            int pick = random.Next(i, cellCount);
            (positions[i], positions[pick]) = (positions[pick], positions[i]);

            int chosen = positions[i];
            grid[chosen / columns][chosen % columns] = (int)CellState.Rotten;
        }

        return grid;
    }
}
