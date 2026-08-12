namespace Interviews.RottingOranges;

/// <summary>
/// Writes each wave to a <see cref="TextWriter"/> as a heading followed by the rendered field.
/// </summary>
/// <remarks>
/// Snapshots accumulate one after another, so the whole run can be read back afterwards.
/// For a display that repaints in place, implement <see cref="IWaveObserver"/> directly.
/// </remarks>
/// <param name="writer">The destination for the snapshots.</param>
public sealed class TextWriterWaveObserver(TextWriter writer) : IWaveObserver
{
    private readonly TextWriter writer = writer
        ?? throw new ArgumentNullException(nameof(writer));

    /// <inheritdoc />
    public async ValueTask OnWaveAsync(Wave wave, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string heading = wave.IsInitial ? $"Minute {wave.Minute} (initial)" : $"Minute {wave.Minute}";
        string remaining = wave.FreshRemaining == 1
            ? "1 fresh orange left"
            : $"{wave.FreshRemaining} fresh oranges left";

        await writer.WriteLineAsync($"{heading} - {remaining}:").ConfigureAwait(false);
        await writer.WriteLineAsync(GridFormatter.Format(wave.Field)).ConfigureAwait(false);
        await writer.WriteLineAsync().ConfigureAwait(false);
    }
}
