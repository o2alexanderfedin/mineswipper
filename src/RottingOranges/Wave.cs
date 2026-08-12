namespace Interviews.RottingOranges;

/// <summary>
/// One observable step of the spread: the state of the field at a given minute.
/// </summary>
/// <param name="Minute">Minutes elapsed. Zero for the snapshot taken before any rot spreads.</param>
/// <param name="Field">A read-only view of the field as it stands at this minute.</param>
/// <param name="FreshRemaining">How many fresh oranges are still standing.</param>
/// <param name="IsInitial">Whether this is the snapshot taken before the first wave.</param>
public readonly record struct Wave(
    int Minute,
    GridView Field,
    int FreshRemaining,
    bool IsInitial);
