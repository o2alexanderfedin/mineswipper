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
    /// <summary>Row/column deltas of the four orthogonally adjacent cells.</summary>
    private static readonly (int Row, int Column)[] NeighbourOffsets =
        [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>
    /// Returns the number of minutes until no cell holds a fresh orange,
    /// or <c>-1</c> if that never happens.
    /// </summary>
    /// <param name="grid">
    /// A rectangular grid whose cells are the integer values of <see cref="CellState"/>.
    /// The caller's array is not modified.
    /// </param>
    /// <returns>
    /// The elapsed minutes, <c>0</c> when the grid starts with no fresh oranges, or
    /// <c>-1</c> when at least one fresh orange is never reached by the rot.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="grid"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="grid"/> is empty, has a null or empty row, or is not rectangular.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A cell holds a value that is not a defined <see cref="CellState"/>.
    /// </exception>
    public static int OrangesRotting(int[][] grid) => SpreadRot(CopyAndValidate(grid));

    /// <summary>
    /// Runs the multi-source breadth-first search over a grid the caller owns exclusively;
    /// cells are marked <see cref="CellState.Rotten"/> in place as the rot spreads.
    /// </summary>
    private static int SpreadRot(CellState[][] cells)
    {
        int rows = cells.Length;
        int columns = cells[0].Length;

        Queue<(int Row, int Column, int Minute)> frontier = new();
        int freshCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                switch (cells[row][column])
                {
                    case CellState.Rotten:
                        frontier.Enqueue((row, column, 0));
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

        while (frontier.Count > 0)
        {
            (int row, int column, int minute) = frontier.Dequeue();
            minutes = Math.Max(minutes, minute);

            foreach ((int rowOffset, int columnOffset) in NeighbourOffsets)
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
                frontier.Enqueue((neighbourRow, neighbourColumn, minute + 1));
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
