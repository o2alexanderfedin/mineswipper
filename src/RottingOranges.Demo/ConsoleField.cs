namespace Interviews.RottingOranges.Demo;

/// <summary>
/// Works out how large a field can be drawn in the console without scrolling.
/// </summary>
/// <remarks>
/// Scrolling is what breaks the animation: the renderer repaints from the top-left corner,
/// so the moment the frame is taller than the window every repaint lands in the wrong place.
/// </remarks>
public static class ConsoleField
{
    /// <summary>Columns a single cell occupies - the emoji used for cells are double-width.</summary>
    public const int CellWidth = 2;

    /// <summary>Smallest field worth drawing, whatever the window reports.</summary>
    public const int MinimumSize = 4;

    /// <summary>Window size assumed when the real one cannot be read.</summary>
    public const int FallbackWidth = 80;

    /// <inheritdoc cref="FallbackWidth"/>
    public const int FallbackHeight = 24;

    /// <summary>
    /// Returns the largest field that fits a window of the given size.
    /// </summary>
    /// <param name="windowWidth">The console width in characters. Non-positive values fall back.</param>
    /// <param name="windowHeight">The console height in lines. Non-positive values fall back.</param>
    /// <param name="reservedLines">
    /// Lines the field must not claim - headings above it and the summary printed below.
    /// </param>
    /// <returns>The field's row and column counts, never smaller than <see cref="MinimumSize"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reservedLines"/> is negative.</exception>
    public static (int Rows, int Columns) Compute(int windowWidth, int windowHeight, int reservedLines)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(reservedLines);

        int width = windowWidth > 0 ? windowWidth : FallbackWidth;
        int height = windowHeight > 0 ? windowHeight : FallbackHeight;

        // Leave the last column unused: a cell that straddles the right edge wraps, and one
        // wrapped line pushes every row below it out of place.
        int columns = (width - 1) / CellWidth;
        int rows = height - reservedLines;

        return (Math.Max(MinimumSize, rows), Math.Max(MinimumSize, columns));
    }

    /// <summary>
    /// Returns the largest field that fits the current console window, falling back to a
    /// conventional 80x24 when the window cannot be measured - which is the case whenever
    /// output is redirected to a file or a pipe.
    /// </summary>
    /// <param name="reservedLines">
    /// Lines the field must not claim - headings above it and the summary printed below.
    /// </param>
    /// <returns>The field's row and column counts.</returns>
    public static (int Rows, int Columns) Measure(int reservedLines)
    {
        int width = 0;
        int height = 0;

        if (!Console.IsOutputRedirected)
        {
            try
            {
                width = Console.WindowWidth;
                height = Console.WindowHeight;
            }
            catch (IOException)
            {
                // No terminal behind the handle; the fallback below applies.
            }
        }

        return Compute(width, height, reservedLines);
    }
}
