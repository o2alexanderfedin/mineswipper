using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Interviews.RottingOranges.Demo;

/// <summary>
/// The demo's command line: an optional seed, then an optional spread pattern.
/// </summary>
/// <remarks>
/// Anything the demo does not recognise is rejected rather than replaced by a default: a
/// misspelled pattern that quietly ran as orthogonal would show the user a different
/// animation from the one they asked for, with nothing to tell them so.
/// </remarks>
/// <param name="Seed">Seeds the field for a reproducible run, or <see langword="null"/> for a random one.</param>
/// <param name="Pattern">Which neighbours the rot reaches each minute.</param>
public sealed record DemoOptions(int? Seed, SpreadPattern Pattern)
{
    /// <summary>The accepted spread patterns, as they are typed on the command line.</summary>
    private static readonly string[] PatternNames =
        [.. Enum.GetNames<SpreadPattern>().Select(name => name.ToLowerInvariant())];

    /// <summary>A one-line description of the accepted arguments.</summary>
    public static string Usage =>
        $"usage: RottingOranges.Demo [seed [{string.Join('|', PatternNames)}]]";

    /// <summary>
    /// Reads the demo's arguments.
    /// </summary>
    /// <param name="args">The command-line arguments, without the program name.</param>
    /// <param name="options">The parsed options, when every argument is recognised.</param>
    /// <param name="error">Which argument was not recognised and why, otherwise.</param>
    /// <returns><see langword="true"/> when every argument is recognised.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/>.</exception>
    public static bool TryParse(
        IReadOnlyList<string> args,
        [NotNullWhen(true)] out DemoOptions? options,
        [NotNullWhen(false)] out string? error)
    {
        ArgumentNullException.ThrowIfNull(args);

        options = null;

        if (args.Count > 2)
        {
            error = $"Unexpected argument '{args[2]}'.";
            return false;
        }

        int? seed = null;

        if (args.Count > 0)
        {
            if (!int.TryParse(args[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value))
            {
                error = $"The seed must be a whole number, but got '{args[0]}'.";
                return false;
            }

            seed = value;
        }

        SpreadPattern pattern = SpreadPattern.Orthogonal;

        // Matched by name only: enum parsing would also accept numbers and comma-separated
        // lists, and hand the solver a value that is no pattern at all.
        if (args.Count > 1 && !TryParsePattern(args[1], out pattern))
        {
            error = $"Unknown spread pattern '{args[1]}'; expected one of: {string.Join(", ", PatternNames)}.";
            return false;
        }

        options = new DemoOptions(seed, pattern);
        error = null;
        return true;
    }

    /// <summary>Matches a pattern by its name, ignoring case.</summary>
    private static bool TryParsePattern(string text, out SpreadPattern pattern)
    {
        foreach (SpreadPattern candidate in Enum.GetValues<SpreadPattern>())
        {
            if (string.Equals(candidate.ToString(), text, StringComparison.OrdinalIgnoreCase))
            {
                pattern = candidate;
                return true;
            }
        }

        pattern = default;
        return false;
    }
}
