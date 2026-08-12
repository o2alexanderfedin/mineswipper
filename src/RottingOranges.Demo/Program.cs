using System.Globalization;
using System.Text;

using Interviews.RottingOranges;
using Interviews.RottingOranges.Demo;

const int Rows = 10;
const int Columns = 20;
const int RottenCount = 1;

TimeSpan frameDelay = TimeSpan.FromMilliseconds(120);

// The field is drawn with emoji, which a console using a legacy code page would mangle.
Console.OutputEncoding = Encoding.UTF8;

// An optional seed makes a run reproducible: `dotnet run -- 1234`.
Random random = args.Length > 0 && int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed)
    ? new Random(seed)
    : Random.Shared;

int[][] field = GridFactory.CreateRandomField(Rows, Columns, RottenCount, random);

bool animate = !Console.IsOutputRedirected;

if (animate)
{
    Console.Write("\u001b[2J\u001b[H\u001b[?25l");
}

int minutes;

try
{
    minutes = await RottingOrangesSolver.OrangesRottingAsync(
        field,
        new AnimatedConsoleObserver(frameDelay, animate));
}
finally
{
    if (animate)
    {
        Console.Write("\u001b[?25h");
    }
}

Console.WriteLine();
Console.WriteLine(minutes < 0
    ? "Result: -1 (fresh oranges remain that the rot can never reach)"
    : $"Result: all {Rows}x{Columns} oranges rotted in {minutes} minutes");
