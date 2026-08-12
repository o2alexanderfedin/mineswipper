namespace Interviews.RottingOranges;

/// <summary>
/// Solves the "Rotting Oranges" problem: every minute, a rotten orange rots each of its
/// orthogonally adjacent fresh oranges.
/// </summary>
/// <remarks>
/// The grid is traversed with a multi-source breadth-first search seeded from every orange
/// that is already rotten, so each cell is visited once:
/// O(rows * columns) time and O(rows * columns) space.
/// </remarks>
public static class RottingOrangesSolver
{
    /// <summary>Row/column deltas of the four cells sharing an edge.</summary>
    private static readonly (int Row, int Column)[] OrthogonalOffsets =
        [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>Row/column deltas of all eight surrounding cells.</summary>
    private static readonly (int Row, int Column)[] DiagonalOffsets =
        [(-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1)];

    /// <summary>Returns the neighbour deltas the given pattern spreads across.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="pattern"/> is not a defined <see cref="SpreadPattern"/>.
    /// </exception>
    private static (int Row, int Column)[] OffsetsFor(SpreadPattern pattern) => pattern switch
    {
        SpreadPattern.Orthogonal => OrthogonalOffsets,
        SpreadPattern.Diagonal => DiagonalOffsets,
        _ => throw new ArgumentOutOfRangeException(
            nameof(pattern),
            pattern,
            "Unknown spread pattern."),
    };

    /// <summary>
    /// Returns the number of minutes until no cell holds a fresh orange,
    /// or <c>-1</c> if that never happens.
    /// </summary>
    /// <param name="grid">
    /// A rectangular grid whose cells are the integer values of <see cref="CellState"/>.
    /// The caller's array is not modified.
    /// </param>
    /// <param name="observer">
    /// Notified before the first wave and after every wave that rots at least one orange.
    /// Pass <see langword="null"/> (the default) to run the search without observation, in
    /// which case it never yields and completes synchronously.
    /// </param>
    /// <param name="pattern">
    /// Which neighbours the rot reaches each minute. Defaults to
    /// <see cref="SpreadPattern.Orthogonal"/>, the original problem's rule.
    /// </param>
    /// <param name="cancellationToken">Signals that the search should be abandoned.</param>
    /// <returns>
    /// The elapsed minutes, <c>0</c> when the grid starts with no fresh oranges, or
    /// <c>-1</c> when at least one fresh orange is never reached by the rot.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="grid"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="grid"/> is empty, has a null or empty row, or is not rectangular.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A cell holds a value that is not a defined <see cref="CellState"/>, or
    /// <paramref name="pattern"/> is not a defined <see cref="SpreadPattern"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
    public static Task<int> OrangesRottingAsync(
        int[][] grid,
        IWaveObserver? observer = null,
        SpreadPattern pattern = SpreadPattern.Orthogonal,
        CancellationToken cancellationToken = default) =>
        SpreadRotAsync(CopyAndValidate(grid), observer, OffsetsFor(pattern), cancellationToken);

    /// <summary>
    /// Runs the search, writing a snapshot of the field to <paramref name="trace"/> before the
    /// first wave and after every wave that rots at least one orange.
    /// </summary>
    /// <param name="grid">The grid to solve. The caller's array is not modified.</param>
    /// <param name="trace">The destination for the snapshots.</param>
    /// <param name="pattern">
    /// Which neighbours the rot reaches each minute. Defaults to
    /// <see cref="SpreadPattern.Orthogonal"/>.
    /// </param>
    /// <param name="cancellationToken">Signals that the search should be abandoned.</param>
    /// <returns>The elapsed minutes, or <c>-1</c>. See the primary overload.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="grid"/> or <paramref name="trace"/> is <see langword="null"/>.
    /// </exception>
    public static Task<int> OrangesRottingAsync(
        int[][] grid,
        TextWriter trace,
        SpreadPattern pattern = SpreadPattern.Orthogonal,
        CancellationToken cancellationToken = default) =>
        OrangesRottingAsync(grid, new TextWriterWaveObserver(trace), pattern, cancellationToken);

    /// <summary>
    /// Runs the multi-source breadth-first search over a grid the caller owns exclusively;
    /// cells are marked <see cref="CellState.Rotten"/> in place as the rot spreads.
    /// </summary>
    /// <remarks>
    /// Two stacks are flipped at each wave boundary: one is drained while everything it rots
    /// is pushed onto the other. A flip therefore means a whole wave - every cell that rots
    /// during the same minute - is finished, which is what advances the clock and gives the
    /// observer a coherent field to render.
    /// </remarks>
    private static async Task<int> SpreadRotAsync(
        CellState[][] cells,
        IWaveObserver? observer,
        (int Row, int Column)[] neighbourOffsets,
        CancellationToken cancellationToken)
    {
        int rows = cells.Length;
        int columns = cells[0].Length;

        Stack<(int Row, int Column)> currentWave = new();
        Stack<(int Row, int Column)> nextWave = new();
        int freshCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                switch (cells[row][column])
                {
                    case CellState.Rotten:
                        currentWave.Push((row, column));
                        break;
                    case CellState.Fresh:
                        freshCount++;
                        break;
                    case CellState.Empty:
                    default:
                        break;
                }
            }
        }

        int minutes = 0;
        GridView field = new(cells);

        if (observer is not null)
        {
            await observer
                .OnWaveAsync(new Wave(minutes, field, freshCount, IsInitial: true), cancellationToken)
                .ConfigureAwait(false);
        }

        while (currentWave.Count > 0 && freshCount > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Drain this minute's oranges, collecting everything they rot onto the other stack.
            // Nothing pushed here can be reached before the flip, so the wave cannot smear
            // into the next one. Order within a wave is irrelevant - every cell in it rots at
            // the same minute - so a stack serves as well as a queue and pops warmer entries.
            while (currentWave.Count > 0)
            {
                (int row, int column) = currentWave.Pop();

                foreach ((int rowOffset, int columnOffset) in neighbourOffsets)
                {
                    int neighbourRow = row + rowOffset;
                    int neighbourColumn = column + columnOffset;

                    if (neighbourRow < 0 || neighbourRow >= rows ||
                        neighbourColumn < 0 || neighbourColumn >= columns)
                    {
                        continue;
                    }

                    if (cells[neighbourRow][neighbourColumn] != CellState.Fresh)
                    {
                        continue;
                    }

                    cells[neighbourRow][neighbourColumn] = CellState.Rotten;
                    freshCount--;
                    nextWave.Push((neighbourRow, neighbourColumn));
                }
            }

            // The flip: one wave is finished, so one minute has passed. The stack just drained
            // is empty, which is what makes it reusable as the next wave's collector.
            (currentWave, nextWave) = (nextWave, currentWave);

            // An empty wave means nothing rotted, so no minute passed and there is nothing new
            // to show. Whatever fresh oranges are left are out of the rot's reach for good.
            if (currentWave.Count == 0)
            {
                break;
            }

            minutes++;

            if (observer is not null)
            {
                await observer
                    .OnWaveAsync(new Wave(minutes, field, freshCount, IsInitial: false), cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return freshCount == 0 ? minutes : -1;
    }

    /// <summary>
    /// Validates the grid's shape and contents and projects it onto a private
    /// <see cref="CellState"/> array, so the search never mutates the caller's input.
    /// </summary>
    private static CellState[][] CopyAndValidate(int[][] grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        if (grid.Length == 0)
        {
            throw new ArgumentException("Grid must contain at least one row.", nameof(grid));
        }

        CellState[][] copy = new CellState[grid.Length][];
        int columns = -1;

        for (int row = 0; row < grid.Length; row++)
        {
            int[]? cells = grid[row];

            if (cells is null)
            {
                throw new ArgumentException($"Grid row {row} is null.", nameof(grid));
            }

            if (row == 0)
            {
                columns = cells.Length;

                if (columns == 0)
                {
                    throw new ArgumentException("Grid must contain at least one column.", nameof(grid));
                }
            }
            else if (cells.Length != columns)
            {
                throw new ArgumentException(
                    $"Grid must be rectangular, but row {row} has {cells.Length} cells instead of {columns}.",
                    nameof(grid));
            }

            CellState[] converted = new CellState[cells.Length];

            for (int column = 0; column < cells.Length; column++)
            {
                CellState state = (CellState)cells[column];

                if (!Enum.IsDefined(state))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(grid),
                        cells[column],
                        $"Grid cell [{row}][{column}] holds an unknown cell value.");
                }

                converted[column] = state;
            }

            copy[row] = converted;
        }

        return copy;
    }
}
