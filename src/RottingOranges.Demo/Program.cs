using System.Globalization;
using System.Text;

using Interviews.RottingOranges;
using Interviews.RottingOranges.Demo;

const int Rows = 20;
const int Columns = 50;
const int RottenCount = 1;

// Roughly an eighth of the field. Far below the percolation threshold, so the field almost
// always stays connected - but small pockets do get sealed off, which is the interesting part.
const int HoleCount = (Rows * Columns) / 8;

TimeSpan frameDelay = TimeSpan.FromMilliseconds(120);

// The field is drawn with emoji, which a console using a legacy code page would mangle.
Console.OutputEncoding = Encoding.UTF8;

// An optional seed makes a run reproducible: `dotnet run -- 1234`.
Random random = args.Length > 0 && int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed)
    ? new Random(seed)
    : Random.Shared;

int[][] field = GridFactory.CreateRandomField(Rows, Columns, RottenCount, HoleCount, random);
int orangeCount = (Rows * Columns) - HoleCount;

bool animate = !Console.IsOutputRedirected;

if (animate)
{
    Console.Write("\u001b[2J\u001b[H\u001b[?25l");
}

int minutes;
AnimatedConsoleObserver observer = new(frameDelay, animate);

try
{
    minutes = await RottingOrangesSolver.OrangesRottingAsync(field, observer);
}
finally
{
    if (animate)
    {
        Console.Write("\u001b[?25h");
    }
}

Console.WriteLine();
Console.WriteLine($"Field: {Rows}x{Columns} with {HoleCount} holes, {orangeCount} oranges");
Console.WriteLine(minutes < 0
    ? $"Result: -1 ({observer.FreshRemaining} orange(s) sealed off by holes, unreachable by rot)"
    : $"Result: all {orangeCount} oranges rotted in {minutes} minutes");
