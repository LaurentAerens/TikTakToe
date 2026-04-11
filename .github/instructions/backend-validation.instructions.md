---
applyTo: "src/backend/**"
description: "Use when: backend code changes, API/data/service updates, migrations or persistence changes. Always run full backend tests including black-box tests and check EF model drift before finishing."
---

# Backend Validation

When files under `src/backend/**` are changed, run these checks before concluding work:

```bash
dotnet test src/backend/TikTakToe.Tests/TikTakToe.Tests.csproj --configuration Release
dotnet ef migrations has-pending-model-changes --project src/backend/TikTakToe/TikTakToe.csproj --startup-project src/backend/TikTakToe/TikTakToe.csproj --context GameDbContext
```

## Expectations

- Do not skip black-box tests unless explicitly requested.
- If tests fail, fix regressions or clearly report blockers.
- Treat pending model changes as a failure unless a migration update is intentional and included.
