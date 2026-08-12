using System.Text;

using Interviews.RottingOranges;

// The field is drawn with emoji, which a console using a legacy code page would mangle.
Console.OutputEncoding = Encoding.UTF8;

(string Name, int[][] Grid)[] scenarios =
[
    ("Every orange is reached", [[2, 1, 1], [1, 1, 0], [0, 1, 1]]),
    ("One orange is walled off", [[2, 1, 1], [0, 1, 1], [1, 0, 1]]),
    ("Two sources, opposite corners", [[2, 1, 1], [1, 1, 1], [1, 1, 2]]),
];

Console.WriteLine($"Legend: {GridFormatter.Legend}");
Console.WriteLine();

foreach ((string name, int[][] grid) in scenarios)
{
    Console.WriteLine($"=== {name} ===");

    int minutes = RottingOrangesSolver.OrangesRotting(grid, Console.Out);

    Console.WriteLine(minutes < 0
        ? "Result: -1 (fresh oranges remain that the rot can never reach)"
        : $"Result: {minutes} minute(s)");
    Console.WriteLine();
}
