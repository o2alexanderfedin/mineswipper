namespace Interviews.RottingOranges;

/// <summary>
/// Receives the field before the first wave of rot and after every wave that rots at least
/// one orange.
/// </summary>
/// <remarks>
/// The search awaits each call, so an observer may pace the spread - an animated renderer
/// awaits <see cref="Task.Delay(TimeSpan, CancellationToken)"/> here to hold each frame on
/// screen.
/// </remarks>
public interface IWaveObserver
{
    /// <summary>
    /// Called once per wave, in order.
    /// </summary>
    /// <param name="wave">The field and the minute it belongs to.</param>
    /// <param name="cancellationToken">Signals that the search should be abandoned.</param>
    /// <returns>A task that completes when the observer is ready for the next wave.</returns>
    ValueTask OnWaveAsync(Wave wave, CancellationToken cancellationToken);
}
