namespace Interviews.RottingOranges.Tests;

/// <summary>
/// Specification for the <see cref="IWaveObserver"/> contract: what the search reports, when,
/// and that the observer can pace or abandon the spread.
/// </summary>
public sealed class WaveObserverTests
{
    /// <summary>Records every wave, rendering each field before it changes again.</summary>
    private sealed class RecordingObserver : IWaveObserver
    {
        public List<(int Minute, string Field, int FreshRemaining, bool IsInitial)> Waves { get; } = [];

        public ValueTask OnWaveAsync(Wave wave, CancellationToken cancellationToken)
        {
            Waves.Add((wave.Minute, GridFormatter.Format(wave.Field), wave.FreshRemaining, wave.IsInitial));
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task Observer_SeesInitialWaveFirstThenOnePerMinute()
    {
        RecordingObserver observer = new();

        int minutes = await RottingOrangesSolver.OrangesRottingAsync([[2, 1, 1]], observer);

        Assert.Equal(2, minutes);
        Assert.Equal([0, 1, 2], observer.Waves.Select(w => w.Minute));
        Assert.Equal([true, false, false], observer.Waves.Select(w => w.IsInitial));
        Assert.Equal([2, 1, 0], observer.Waves.Select(w => w.FreshRemaining));
    }

    [Fact]
    public async Task Observer_SeesTheFieldAsItStoodAtEachWave()
    {
        RecordingObserver observer = new();

        _ = await RottingOrangesSolver.OrangesRottingAsync([[2, 1, 1]], observer);

        const string rotten = GridFormatter.RottenSymbol;
        const string fresh = GridFormatter.FreshSymbol;

        Assert.Equal(
            [rotten + fresh + fresh, rotten + rotten + fresh, rotten + rotten + rotten],
            observer.Waves.Select(w => w.Field));
    }

    [Fact]
    public async Task Observer_IsNotCalledForAWaveThatRotsNothing()
    {
        RecordingObserver observer = new();

        int minutes = await RottingOrangesSolver.OrangesRottingAsync([[2, 0, 1]], observer);

        Assert.Equal(-1, minutes);
        Assert.Single(observer.Waves);
        Assert.True(observer.Waves[0].IsInitial);
    }

    [Fact]
    public async Task Observer_CanPaceTheSpread()
    {
        // Each wave awaits a real delay, so the run cannot finish faster than their sum.
        TimeSpan perWave = TimeSpan.FromMilliseconds(20);
        int waves = 0;

        DelegatingObserver observer = new(async (_, token) =>
        {
            waves++;
            await Task.Delay(perWave, token);
        });

        long startedAt = Environment.TickCount64;
        int minutes = await RottingOrangesSolver.OrangesRottingAsync([[2, 1, 1, 1]], observer);
        long elapsed = Environment.TickCount64 - startedAt;

        Assert.Equal(3, minutes);
        Assert.Equal(4, waves);
        Assert.True(
            elapsed >= perWave.TotalMilliseconds * waves,
            $"expected at least {perWave.TotalMilliseconds * waves}ms of pacing, took {elapsed}ms");
    }

    [Fact]
    public async Task Observer_CanAbandonTheSearch()
    {
        using CancellationTokenSource cancellation = new();
        int waves = 0;

        DelegatingObserver observer = new((_, _) =>
        {
            waves++;

            if (waves == 2)
            {
                cancellation.Cancel();
            }

            return ValueTask.CompletedTask;
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => RottingOrangesSolver.OrangesRottingAsync(
                GridFactory.CreateRandomField(10, 20, 1, 0, new Random(1)),
                observer,
                cancellation.Token));

        Assert.Equal(2, waves);
    }

    [Fact]
    public async Task Observer_CancellationIsObservedBeforeAnyWaveWhenAlreadyCancelled()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        RecordingObserver observer = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => RottingOrangesSolver.OrangesRottingAsync([[2, 1, 1]], observer, cancellation.Token));
    }

    [Fact]
    public async Task Observer_FieldViewReportsTheGridDimensions()
    {
        int rows = 0;
        int columns = 0;

        DelegatingObserver observer = new((wave, _) =>
        {
            rows = wave.Field.Rows;
            columns = wave.Field.Columns;
            return ValueTask.CompletedTask;
        });

        _ = await RottingOrangesSolver.OrangesRottingAsync(
            GridFactory.CreateRandomField(10, 20, 1, 0, new Random(7)),
            observer);

        Assert.Equal(10, rows);
        Assert.Equal(20, columns);
    }

    [Fact]
    public async Task TextWriterWaveObserver_ThrowsWhenWriterIsNull()
    {
        await Task.CompletedTask;
        Assert.Throws<ArgumentNullException>(() => new TextWriterWaveObserver(null!));
    }

    /// <summary>An observer that forwards each wave to a callback.</summary>
    private sealed class DelegatingObserver(Func<Wave, CancellationToken, ValueTask> onWave) : IWaveObserver
    {
        public ValueTask OnWaveAsync(Wave wave, CancellationToken cancellationToken) =>
            onWave(wave, cancellationToken);
    }
}
