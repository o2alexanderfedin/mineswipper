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

With no observer the search never yields and completes synchronously. Pass a
`TextWriter` instead to append a snapshot per wave:

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

Floods a 20×50 field from a randomly placed rotten orange, repainting in place once per
minute. A quarter of the cells are carved into a dozen or so caves the rot cannot cross:

```bash
dotnet run --project src/RottingOranges.Demo          # random field
dotnet run --project src/RottingOranges.Demo -- 2024  # reproducible: seeds the field
```

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
