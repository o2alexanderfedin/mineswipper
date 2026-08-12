using System.Globalization;
using System.Text;

using Humanizer;

using Interviews.RottingOranges;
using Interviews.RottingOranges.Demo;

const int Rows = 20;
const int Columns = 50;
const int RottenCount = 1;

// A quarter of the field, carved as many separate caves. Blobs that touch merge, so the
// nominal size is deliberately well under what a cave ends up being: at this density these
// settle at roughly a dozen holes averaging twice HoleSize.
const int HoleCells = (Rows * Columns) / 4;
const int HoleSize = 10;

TimeSpan frameDelay = 120.Milliseconds();

// The field is drawn with emoji, which a console using a legacy code page would mangle.
Console.OutputEncoding = Encoding.UTF8;

// An optional seed makes a run reproducible: `dotnet run -- 1234`.
Random random = args.Length > 0 && int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed)
    ? new Random(seed)
    : Random.Shared;

int[][] field = GridFactory.CreateRandomField(Rows, Columns, RottenCount, HoleCells, HoleSize, random);
int orangeCount = (Rows * Columns) - HoleCells;

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
Console.WriteLine($"Field: {Rows}x{Columns}, {HoleCells} cells carved into ~{HoleCells / HoleSize} holes, {orangeCount} oranges");
Console.WriteLine(minutes < 0
    ? $"Result: -1 ({observer.FreshRemaining} orange(s) sealed off by holes, unreachable by rot)"
    : $"Result: all {orangeCount} oranges rotted in {minutes} minutes");
