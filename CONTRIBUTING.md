# Contributing

Thank you for your interest in contributing.

We welcome bug reports, feature ideas, documentation improvements, and pull requests.

## Be Kind

Please help keep this project welcoming and constructive.

- Be respectful and assume good intent.
- Give feedback on code, not on people.
- Be patient with different experience levels.
- Prefer clear, helpful suggestions over criticism.
- If you disagree, stay factual and polite.

## Ways to Contribute

- Report bugs and include clear reproduction steps.
- Suggest improvements and explain the use case.
- Improve documentation when something is unclear.
- Open pull requests for fixes and enhancements.

## Getting Started

1. Fork the repository.
2. Create a branch for your change.
3. Make focused, minimal changes.
4. Add or update tests when behavior changes.
5. Update docs where needed.
6. Open a pull request with a clear summary.

## Pull Request Guidelines

- Keep changes small and easy to review.
- Use clear commit messages.
- Link related issues when applicable.
- Describe what changed and why.
- Include test notes and validation steps.

## Local StyleCop Checks (Backend)

Run these commands before opening a pull request when touching backend C# code.

1. Restore once:

```powershell
dotnet restore src/backend/TikTakToe.slnx
```

2. Strict check (mirrors CI blocker behavior):

```powershell
dotnet build src/backend/TikTakToe.slnx -c Release -p:EnforceCodeStyleInBuild=true -p:TreatWarningsAsErrors=false -p:WarningsAsErrors=SA1000%3BSA1001%3BSA1002%3BSA1005%3BSA1008%3BSA1009%3BSA1011%3BSA1018%3BSA1019%3BSA1025%3BSA1028%3BSA1100%3BSA1106%3BSA1108%3BSA1110%3BSA1111%3BSA1115%3BSA1116%3BSA1117%3BSA1127%3BSA1128%3BSA1649 --no-restore --verbosity normal
```

Note: `%3B` encodes semicolons so PowerShell passes `WarningsAsErrors` as one MSBuild property value.
Maintenance: keep this curated blocker list synchronized with `.github/workflows/code-quality.yml` (`stylecopanalyzer` build step).

3. Audit check (non-blocking inventory of additional style diagnostics):

```powershell
dotnet build src/backend/TikTakToe.slnx -c Release -p:EnforceCodeStyleInBuild=true -p:TreatWarningsAsErrors=false --no-restore --verbosity normal
```

## Reporting Issues

When creating an issue, please include:

- Expected behavior.
- Actual behavior.
- Steps to reproduce.
- Relevant logs, screenshots, or request samples.
- Environment details (OS, runtime, version).

## Review and Merge

Maintainers will review contributions as time allows.

You may be asked to make updates before merge. This is normal and helps keep quality high.

## Questions

If you are unsure how to approach a change, open an issue first and we can discuss the best path.

Thanks for helping improve this project.
