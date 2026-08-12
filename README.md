# mineswipper

Interview exercises in C# (.NET 10).

| Project | Contents |
|---------|----------|
| `src/RottingOranges` | Multi-source BFS solution to "Rotting Oranges" (LeetCode 994) |
| `tests/RottingOranges.Tests` | xUnit specification for the above |

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
