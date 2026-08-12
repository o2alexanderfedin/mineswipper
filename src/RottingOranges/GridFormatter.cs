namespace Interviews.RottingOranges;

/// <summary>
/// Renders a grid of <see cref="CellState"/> values as human-readable text.
/// </summary>
/// <remarks>
/// Cells are drawn with emoji. Each symbol is a single double-width glyph, so rows stay
/// aligned in a terminal; they are <see cref="string"/> rather than <see cref="char"/>
/// because the code points sit outside the basic multilingual plane.
/// </remarks>
public static class GridFormatter
{
    /// <summary>The symbol used for <see cref="CellState.Empty"/>.</summary>
    public const string EmptySymbol = "⬛";

    /// <summary>The symbol used for <see cref="CellState.Fresh"/>.</summary>
    public const string FreshSymbol = "\U0001f34a";

    /// <summary>The symbol used for <see cref="CellState.Rotten"/>.</summary>
    public const string RottenSymbol = "\U0001f7e4";

    /// <summary>A one-line description of the symbols used by <see cref="Format"/>.</summary>
    public static string Legend =>
        $"{EmptySymbol} empty   {FreshSymbol} fresh   {RottenSymbol} rotten";

    /// <summary>
    /// Returns the symbol representing a single cell.
    /// </summary>
    /// <param name="state">The cell to render.</param>
    /// <returns>The symbol for <paramref name="state"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="state"/> is not a defined <see cref="CellState"/>.
    /// </exception>
    public static string ToSymbol(CellState state) => state switch
    {
        CellState.Empty => EmptySymbol,
        CellState.Fresh => FreshSymbol,
        CellState.Rotten => RottenSymbol,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown cell state."),
    };

    /// <summary>
    /// Renders the whole grid, one line per row, without a trailing line break.
    /// </summary>
    /// <param name="cells">The grid to render.</param>
    /// <returns>The rendered grid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cells"/> is <see langword="null"/>.</exception>
    public static string Format(CellState[][] cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        return string.Join(
            Environment.NewLine,
            cells.Select(row => string.Concat(row.Select(ToSymbol))));
    }
}
