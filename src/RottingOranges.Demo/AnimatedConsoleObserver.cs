using System.Text;

using Interviews.RottingOranges;

namespace Interviews.RottingOranges.Demo;

/// <summary>
/// Animates the spread by repainting the field in place, one frame per wave.
/// </summary>
/// <remarks>
/// Each frame is redrawn from the top-left rather than clearing the screen, which avoids the
/// flicker a full clear produces. When output is redirected the escape sequences would be
/// noise, so frames are simply appended instead and the pause is skipped.
/// </remarks>
/// <param name="delay">How long each frame stays on screen.</param>
/// <param name="animate">
/// Whether to repaint in place and pause. Defaults to whether the console is a live terminal.
/// </param>
internal sealed class AnimatedConsoleObserver(TimeSpan delay, bool? animate = null) : IWaveObserver
{
    private const string CursorHome = "\u001b[H";
    private const string EraseToEndOfLine = "\u001b[K";
    private const string EraseBelow = "\u001b[J";

    private readonly bool animate = animate ?? !Console.IsOutputRedirected;

    /// <inheritdoc />
    public async ValueTask OnWaveAsync(Wave wave, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        StringBuilder frame = new();

        if (animate)
        {
            frame.Append(CursorHome);
        }

        AppendLine(frame, $"Legend: {GridFormatter.Legend}");
        AppendLine(frame, Describe(wave));
        AppendLine(frame, string.Empty);

        foreach (string line in GridFormatter.FormatLines(wave.Field))
        {
            AppendLine(frame, line);
        }

        if (animate)
        {
            frame.Append(EraseBelow);
        }
        else
        {
            frame.AppendLine();
        }

        Console.Out.Write(frame.ToString());
        await Console.Out.FlushAsync(cancellationToken).ConfigureAwait(false);

        if (animate)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Builds the heading shown above the field.</summary>
    private static string Describe(Wave wave)
    {
        string heading = wave.IsInitial ? $"Minute {wave.Minute} (initial)" : $"Minute {wave.Minute}";
        string remaining = wave.FreshRemaining == 1
            ? "1 fresh orange left"
            : $"{wave.FreshRemaining} fresh oranges left";

        return $"{heading} - {remaining}";
    }

    /// <summary>
    /// Appends a line, erasing whatever the previous frame left on it when animating.
    /// </summary>
    private void AppendLine(StringBuilder frame, string text)
    {
        frame.Append(text);

        if (animate)
        {
            frame.Append(EraseToEndOfLine);
        }

        frame.Append('\n');
    }
}
