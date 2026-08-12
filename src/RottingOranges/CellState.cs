namespace Interviews.RottingOranges;

/// <summary>
/// The three states a cell of the orange grid may hold.
/// </summary>
/// <remarks>
/// The underlying values match the integer encoding used by the problem statement, so an
/// <c>int[][]</c> grid converts to <see cref="CellState"/> by a direct cast once validated.
/// </remarks>
public enum CellState
{
    /// <summary>An empty cell. Rot cannot pass through it.</summary>
    Empty = 0,

    /// <summary>A fresh orange. Rots one minute after an orthogonal neighbour rots.</summary>
    Fresh = 1,

    /// <summary>A rotten orange. Acts as a source for the breadth-first spread of rot.</summary>
    Rotten = 2,
}
