namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Specification for the per-wave field snapshots written by
/// <see cref="RottingOrangesSolver.OrangesRottingAsync(int[][], TextWriter, CancellationToken)"/>.
/// </summary>
public sealed class RottingOrangesSolverTraceTests
{
    private static async Task<(int Minutes, string Trace)> RunAsync(int[][] grid)
    {
        await using StringWriter writer = new();
        int minutes = await RottingOrangesSolver.OrangesRottingAsync(grid, writer);
        return (minutes, writer.ToString());
    }

    /// <summary>Counts the snapshot headings emitted for a run.</summary>
    private static int CountSnapshots(string trace) =>
        trace.Split("Minute ", StringSplitOptions.None).Length - 1;

    [Fact]
    public async Task Trace_EmitsInitialSnapshotPlusOnePerWave()
    {
        // Rot needs two waves to consume this grid.
        (int minutes, string trace) = await RunAsync([[2, 1], [1, 1]]);

        Assert.Equal(2, minutes);
        Assert.Equal(3, CountSnapshots(trace));
        Assert.Contains("Minute 0 (initial)", trace, StringComparison.Ordinal);
        Assert.Contains("Minute 1", trace, StringComparison.Ordinal);
        Assert.Contains("Minute 2", trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_RendersEachWaveOfTheField()
    {
        (int minutes, string trace) = await RunAsync([[2, 1, 1]]);

        Assert.Equal(2, minutes);

        const string rotten = GridFormatter.RottenSymbol;
        const string fresh = GridFormatter.FreshSymbol;

        // The single row rots one cell further to the right each minute.
        Assert.Contains(rotten + fresh + fresh, trace, StringComparison.Ordinal);
        Assert.Contains(rotten + rotten + fresh, trace, StringComparison.Ordinal);
        Assert.Contains(rotten + rotten + rotten, trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_ReportsRemainingFreshCount()
    {
        (_, string trace) = await RunAsync([[2, 1, 1]]);

        Assert.Contains("Minute 0 (initial) - 2 fresh oranges left:", trace, StringComparison.Ordinal);
        Assert.Contains("Minute 1 - 1 fresh orange left:", trace, StringComparison.Ordinal);
        Assert.Contains("Minute 2 - 0 fresh oranges left:", trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_EmitsNoWaveWhenTheRotCannotSpreadAtAll()
    {
        // The only fresh orange sits behind an empty cell, so the first wave rots nothing.
        (int minutes, string trace) = await RunAsync([[2, 0, 1]]);

        Assert.Equal(-1, minutes);
        Assert.Equal(1, CountSnapshots(trace));
        Assert.Contains("Minute 0 (initial) - 1 fresh orange left:", trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_OmitsTheFinalWaveThatChangesNothing()
    {
        // Rot spreads for four minutes, then stalls with one unreachable orange left.
        (int minutes, string trace) = await RunAsync([[2, 1, 1], [0, 1, 1], [1, 0, 1]]);

        Assert.Equal(-1, minutes);

        // Initial snapshot plus the four waves that actually rotted something.
        Assert.Equal(5, CountSnapshots(trace));
        Assert.Contains("Minute 4 - 1 fresh orange left:", trace, StringComparison.Ordinal);
        Assert.DoesNotContain("Minute 5", trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_NeverRepeatsTheSameFieldTwice()
    {
        (_, string trace) = await RunAsync([[2, 1, 1], [0, 1, 1], [1, 0, 1]]);

        string[] snapshots = trace
            .Split("Minute ", StringSplitOptions.RemoveEmptyEntries)
            .Select(block => block[(block.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim())
            .ToArray();

        Assert.Equal(snapshots.Length, snapshots.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task Trace_EmitsOnlyTheInitialSnapshotWhenNothingCanRot()
    {
        (int minutes, string trace) = await RunAsync([[2, 2], [2, 2]]);

        Assert.Equal(0, minutes);
        Assert.Equal(1, CountSnapshots(trace));
        Assert.Contains("Minute 0 (initial) - 0 fresh oranges left:", trace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Trace_DoesNotAlterTheReturnedResult()
    {
        int[][] grid = [[2, 1, 1], [1, 1, 0], [0, 1, 1]];

        await using StringWriter writer = new();

        Assert.Equal(
            await RottingOrangesSolver.OrangesRottingAsync(grid),
            await RottingOrangesSolver.OrangesRottingAsync(grid, writer));
    }

    [Fact]
    public async Task Trace_LeavesCallerGridUnchanged()
    {
        int[][] grid = [[2, 1], [1, 1]];

        await using StringWriter writer = new();
        _ = await RottingOrangesSolver.OrangesRottingAsync(grid, writer);

        Assert.Equal([2, 1], grid[0]);
        Assert.Equal([1, 1], grid[1]);
    }

    [Fact]
    public async Task OrangesRotting_WritesNothingWhenNoObserverSupplied()
    {
        // The default overload must stay silent - guards against a stray Console.WriteLine.
        TextWriter original = Console.Out;
        await using StringWriter captured = new();

        try
        {
            Console.SetOut(captured);
            _ = await RottingOrangesSolver.OrangesRottingAsync([[2, 1, 1], [1, 1, 0], [0, 1, 1]]);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal(string.Empty, captured.ToString());
    }

    [Fact]
    public async Task Trace_ThrowsWhenWriterIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => RottingOrangesSolver.OrangesRottingAsync([[2, 1]], (TextWriter)null!));
    }
}
