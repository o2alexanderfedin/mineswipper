namespace Interviews.RottingOranges;

/// <summary>
/// A read-only window onto the search's working grid.
/// </summary>
/// <remarks>
/// Handed to an <see cref="IWaveObserver"/> so it can render the field without copying it and
/// without being able to corrupt the search in progress. The view stays live: it reflects the
/// grid as it is at the moment it is read, so an observer that needs to keep a wave around
/// must render or copy it before returning.
/// </remarks>
public readonly struct GridView : IEquatable<GridView>
{
    private readonly CellState[][] cells;

    /// <summary>Wraps the solver's working grid.</summary>
    internal GridView(CellState[][] cells) => this.cells = cells;

    /// <summary>The number of rows in the field.</summary>
    public int Rows => cells?.Length ?? 0;

    /// <summary>The number of columns in the field.</summary>
    public int Columns => cells is { Length: > 0 } ? cells[0].Length : 0;

    /// <summary>Reads a single cell.</summary>
    /// <param name="row">The zero-based row.</param>
    /// <param name="column">The zero-based column.</param>
    /// <returns>The state of the cell.</returns>
    public CellState this[int row, int column] => cells[row][column];

    /// <summary>Renders the field, one line per row. See <see cref="GridFormatter.Format(GridView)"/>.</summary>
    /// <returns>The rendered field.</returns>
    public override string ToString() => GridFormatter.Format(this);

    /// <summary>Determines whether this view wraps the same grid as <paramref name="other"/>.</summary>
    /// <param name="other">The view to compare with.</param>
    /// <returns><see langword="true"/> when both views wrap the same grid instance.</returns>
    public bool Equals(GridView other) => ReferenceEquals(cells, other.cells);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GridView other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => cells?.GetHashCode() ?? 0;

    /// <summary>Compares two views by the grid they wrap.</summary>
    /// <param name="left">The first view.</param>
    /// <param name="right">The second view.</param>
    /// <returns><see langword="true"/> when both views wrap the same grid instance.</returns>
    public static bool operator ==(GridView left, GridView right) => left.Equals(right);

    /// <summary>Compares two views by the grid they wrap.</summary>
    /// <param name="left">The first view.</param>
    /// <param name="right">The second view.</param>
    /// <returns><see langword="true"/> when the views wrap different grid instances.</returns>
    public static bool operator !=(GridView left, GridView right) => !left.Equals(right);
}
