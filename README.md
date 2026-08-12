# mineswipper

Interview exercises in C# (.NET 10).

| Project | Contents |
|---------|----------|
| `src/RottingOranges` | Multi-source BFS solution to "Rotting Oranges" (LeetCode 994) |
| `src/RottingOranges.Demo` | Console runner that prints the field after each wave |
| `tests/RottingOranges.Tests` | xUnit specification for the above |

## Watching the rot spread

`OrangesRotting` takes an optional `TextWriter`. Supply one and it renders the field
before the first wave and after every wave that rots at least one orange:

```bash
dotnet run --project src/RottingOranges.Demo
```

```text
Legend: ⬛ empty   🍊 fresh   🟤 rotten

Minute 0 (initial) - 6 fresh oranges left:
🟤🍊🍊
🍊🍊⬛
⬛🍊🍊

Minute 1 - 4 fresh oranges left:
🟤🟤🍊
🟤🍊⬛
⬛🍊🍊
```

The library never touches `Console` itself — pass `Console.Out` to print, a
`StringWriter` to capture, or nothing at all to stay silent.

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
