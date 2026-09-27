# mineswipper

Interview exercises in C# (.NET 10).

| Project | Contents |
|---------|----------|
| `src/RottingOranges` | Multi-source BFS solution to "Rotting Oranges" (LeetCode 994) |
| `src/RottingOranges.Demo` | Animated console runner |
| `tests/RottingOranges.Tests` | xUnit specification for the above |

## Watching the rot spread

The search is asynchronous and reports one `Wave` per minute to an optional
`IWaveObserver`, which it awaits — so an observer can pace the spread, render it, or
cancel it:

```csharp
int minutes = await RottingOrangesSolver.OrangesRottingAsync(grid, observer, cancellationToken);
```

With no observer the search never yields and completes synchronously.

`SpreadPattern` controls which neighbours the rot reaches each minute — the four sharing an
edge (the original rule, and the default), or all eight including the corners:

```csharp
await RottingOrangesSolver.OrangesRottingAsync(grid, pattern: SpreadPattern.Diagonal);
```

Diagonal spread strictly adds moves, so it never reaches less than orthogonal and never
takes longer — it can cut across a corner gap that would otherwise stop the rot dead.

Pass a `TextWriter` instead of an observer to append a snapshot per wave:

```csharp
int minutes = await RottingOrangesSolver.OrangesRottingAsync(grid, Console.Out);
```

```text
Minute 0 (initial) - 6 fresh oranges left:
🟤🍊🍊
🍊🍊⬛
⬛🍊🍊

Minute 1 - 4 fresh oranges left:
🟤🟤🍊
🟤🍊⬛
⬛🍊🍊
```

The library never touches `Console` itself — the demo owns all rendering.

## Animated demo

Floods a randomly placed rotten orange across a field sized to fill your terminal,
repainting in place once per minute. A quarter of the cells are carved into caves the rot
cannot cross:

```bash
dotnet run --project src/RottingOranges.Demo                   # random field
dotnet run --project src/RottingOranges.Demo -- 2024           # reproducible: seeds the field
dotnet run --project src/RottingOranges.Demo -- 2024 diagonal  # let the rot cut corners
```

The arguments are positional: a whole-number seed, then `orthogonal` or `diagonal`. Anything
else - a misspelled pattern, a number in the pattern's place, a pattern without a seed -
stops the demo with a one-line error and the usage, exit code 2, before the screen is touched.

The same field, both patterns — diagonal reaches past the walls that trap orthogonal rot:

```text
seed 3   orthogonal: -1 (452 oranges sealed off)   diagonal: -1 (5 sealed off)
seed 11  orthogonal: -1 (1 orange sealed off)      diagonal: all 498 rotted in 42 minutes
```

The field claims the whole window, minus the headings above it and the summary below.
Cells are double-width emoji, so a column is two characters and the last one is left free —
a cell straddling the right edge would wrap and knock every row below it out of place.
Resize the terminal and rerun to get a different field:

| Window | Field | Frames at 120 ms |
|--------|-------|------------------|
| 80×24  | 17×39 | 61 (~7 s) |
| 120×40 | 33×59 | — |
| 200×50 | 43×99 | 134 (~17 s) |

Redirected output has no window to measure, so it falls back to 80×24.

```text
Legend: ⬛ empty   🍊 fresh   🟤 rotten
Minute 6 - 848 fresh oranges left

🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊🍊⬛🍊🍊
🍊⬛🍊🍊🍊🍊🍊🟤🟤🟤⬛🍊🍊⬛🍊🍊🍊🍊⬛🍊
🍊🍊🍊🍊🍊🍊🟤🟤🟤🟤🟤⬛🍊⬛🍊🍊🍊🍊🍊🍊
🍊🍊🍊🍊⬛🍊🍊🟤🟤🟤🍊🍊⬛🍊🍊🍊🍊🍊🍊🍊
...
```

`GridFactory.CreateRandomField` takes the total hole area and a `holeSize`, and grows each
hole outwards from a random seed until it reaches that size — so the carved area is exact
whether it lands as speckle (`holeSize: 1`) or as caves (`holeSize: 25`). Holes that grow
into each other simply merge.

Hole *count* falls as density rises, because neighbouring blobs merge on contact — so a
smaller `holeSize` at higher coverage yields more caves, not fewer. At a quarter coverage,
`holeSize: 10` settles at roughly a dozen holes averaging about twice that.

At this density the field always fragments: across 25 seeds, none rotted the whole field,
23 stranded a handful of oranges and 2 sealed off the source itself. The demo reports how
many were stranded, and the final frame shows them still fresh behind their walls. Lower
`HoleCells` or raise `RottenCount` if you would rather see the field rot completely.

Frames are held with `await Task.Delay`. When output is redirected the escape sequences
would be noise, so frames are appended instead and the pause is skipped — which is what
makes the animation pipe-safe.

## Setup

```bash
git clone https://github.com/o2alexanderfedin/mineswipper.git
cd mineswipper
git config core.hooksPath .githooks   # enable branch-protection hooks

dotnet build
dotnet test
```

Every project inherits `Directory.Build.props`: nullable reference types enabled and
all warnings treated as errors.

## Development

This project follows [git-flow](https://nvie.com/posts/a-successful-git-branching-model/).

| Branch      | Purpose                  |
|-------------|--------------------------|
| `main`      | Production releases      |
| `develop`   | Integration branch       |
| `feature/*` | New features             |
| `release/*` | Release preparation      |
| `hotfix/*`  | Production fixes         |
| `bugfix/*`  | Fixes on `develop`       |

`main` and `develop` are protected — direct commits are rejected by the
`.githooks/pre-commit` hook. Work happens on branches:

```bash
git flow feature start <feature-name>
# make changes, commit
git flow feature finish <feature-name>
```

Releases:

```bash
git flow release start <version>
git flow release finish <version>
```

## License

Not yet specified.
