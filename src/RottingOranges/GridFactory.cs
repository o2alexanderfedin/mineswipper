namespace Interviews.RottingOranges;

/// <summary>
/// Builds starting fields for demonstrations and tests.
/// </summary>
public static class GridFactory
{
    /// <summary>Row/column deltas of the four orthogonally adjacent cells.</summary>
    private static readonly (int Row, int Column)[] NeighbourOffsets =
        [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>
    /// Returns a field of fresh oranges with <paramref name="rottenCount"/> of them replaced by
    /// rotten ones, and <paramref name="holeCells"/> cells carved out as holes.
    /// </summary>
    /// <param name="rows">The number of rows. Must be positive.</param>
    /// <param name="columns">The number of columns. Must be positive.</param>
    /// <param name="rottenCount">How many oranges start out rotten.</param>
    /// <param name="holeCells">
    /// How many cells in total are empty. Rot cannot pass through a hole, so enough of them
    /// will strand oranges the rot can never reach.
    /// </param>
    /// <param name="holeSize">
    /// How many cells each hole is grown to. <c>1</c> scatters single cells; larger values
    /// carve contiguous blobs. The total carved area stays <paramref name="holeCells"/>
    /// either way - this only controls how that area is clumped.
    /// </param>
    /// <param name="random">
    /// The source of randomness. Seed it to reproduce a particular field.
    /// </param>
    /// <returns>A grid of <see cref="CellState"/> values encoded as integers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A dimension is not positive, a count is negative, <paramref name="holeSize"/> is less
    /// than one, or the counts together do not fit the field.
    /// </exception>
    public static int[][] CreateRandomField(
        int rows,
        int columns,
        int rottenCount,
        int holeCells,
        int holeSize,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(rottenCount);
        ArgumentOutOfRangeException.ThrowIfNegative(holeCells);
        ArgumentOutOfRangeException.ThrowIfLessThan(holeSize, 1);

        int cellCount = rows * columns;

        if (rottenCount + (long)holeCells > cellCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(holeCells),
                holeCells,
                $"A {rows}x{columns} field holds {cellCount} cells, which cannot fit " +
                $"{rottenCount} rotten oranges and {holeCells} hole cells.");
        }

        int[][] grid = new int[rows][];

        for (int row = 0; row < rows; row++)
        {
            grid[row] = new int[columns];
            Array.Fill(grid[row], (int)CellState.Fresh);
        }

        // Cells are claimed from a pool that only ever hands out untouched ones, so rotten
        // oranges and holes never overlap and no draw is ever retried.
        CellPool pool = new(cellCount);

        for (int i = 0; i < rottenCount; i++)
        {
            Write(grid, columns, pool.TakeRandom(random), CellState.Rotten);
        }

        CarveHoles(grid, columns, pool, holeCells, holeSize, random);

        return grid;
    }

    /// <summary>
    /// Carves <paramref name="holeCells"/> cells as holes, growing each hole outwards from a
    /// random seed until it reaches <paramref name="holeSize"/> cells.
    /// </summary>
    /// <remarks>
    /// A hole that runs out of room - boxed in by rotten oranges or by holes carved earlier -
    /// simply stops short, and the shortfall is made up by starting another hole elsewhere.
    /// That keeps the carved area exact no matter how the blobs land.
    /// </remarks>
    private static void CarveHoles(
        int[][] grid,
        int columns,
        CellPool pool,
        int holeCells,
        int holeSize,
        Random random)
    {
        int carved = 0;
        List<int> frontier = [];

        while (carved < holeCells && pool.Remaining > 0)
        {
            int target = Math.Min(holeSize, holeCells - carved);

            int seed = pool.TakeRandom(random);
            Write(grid, columns, seed, CellState.Empty);
            carved++;

            frontier.Clear();
            AddNeighbours(frontier, pool, grid, columns, seed);

            for (int grown = 1; grown < target && frontier.Count > 0; grown++)
            {
                // Draw from the whole frontier rather than the most recent cell, which grows
                // rounded blobs instead of the thin snakes a random walk would leave.
                int pick = random.Next(frontier.Count);
                int cell = frontier[pick];
                frontier[pick] = frontier[^1];
                frontier.RemoveAt(frontier.Count - 1);

                if (!pool.TryTake(cell))
                {
                    grown--;
                    continue;
                }

                Write(grid, columns, cell, CellState.Empty);
                carved++;

                AddNeighbours(frontier, pool, grid, columns, cell);
            }
        }
    }

    /// <summary>Queues the untouched orthogonal neighbours of <paramref name="cell"/>.</summary>
    private static void AddNeighbours(
        List<int> frontier,
        CellPool pool,
        int[][] grid,
        int columns,
        int cell)
    {
        int row = cell / columns;
        int column = cell % columns;

        foreach ((int rowOffset, int columnOffset) in NeighbourOffsets)
        {
            int neighbourRow = row + rowOffset;
            int neighbourColumn = column + columnOffset;

            if (neighbourRow < 0 || neighbourRow >= grid.Length ||
                neighbourColumn < 0 || neighbourColumn >= columns)
            {
                continue;
            }

            int neighbour = (neighbourRow * columns) + neighbourColumn;

            if (pool.Contains(neighbour))
            {
                frontier.Add(neighbour);
            }
        }
    }

    /// <summary>Writes a state into the flattened <paramref name="cell"/> position.</summary>
    private static void Write(int[][] grid, int columns, int cell, CellState state) =>
        grid[cell / columns][cell % columns] = (int)state;

    /// <summary>
    /// The cells nothing has claimed yet, supporting a uniform random draw and the removal of
    /// a named cell, both in constant time.
    /// </summary>
    /// <remarks>
    /// Claimed cells are swapped to the front of <see cref="cells"/> and hidden behind
    /// <see cref="claimed"/>, with <see cref="slotOf"/> tracking where each cell currently
    /// sits so a specific one can be found without searching.
    /// </remarks>
    private sealed class CellPool
    {
        private readonly int[] cells;
        private readonly int[] slotOf;
        private int claimed;

        public CellPool(int cellCount)
        {
            cells = new int[cellCount];
            slotOf = new int[cellCount];

            for (int cell = 0; cell < cellCount; cell++)
            {
                cells[cell] = cell;
                slotOf[cell] = cell;
            }
        }

        /// <summary>How many cells remain unclaimed.</summary>
        public int Remaining => cells.Length - claimed;

        /// <summary>Whether <paramref name="cell"/> is still unclaimed.</summary>
        public bool Contains(int cell) => slotOf[cell] >= claimed;

        /// <summary>Claims a uniformly chosen unclaimed cell.</summary>
        public int TakeRandom(Random random) => TakeAt(random.Next(claimed, cells.Length));

        /// <summary>Claims <paramref name="cell"/> if it has not been claimed already.</summary>
        public bool TryTake(int cell)
        {
            if (!Contains(cell))
            {
                return false;
            }

            TakeAt(slotOf[cell]);
            return true;
        }

        private int TakeAt(int slot)
        {
            int cell = cells[slot];
            Swap(slot, claimed);
            claimed++;
            return cell;
        }

        private void Swap(int left, int right)
        {
            (cells[left], cells[right]) = (cells[right], cells[left]);
            slotOf[cells[left]] = left;
            slotOf[cells[right]] = right;
        }
    }
}
