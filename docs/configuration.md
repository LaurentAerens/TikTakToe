# Configuration Reference

This page describes every environment variable and application setting recognised by the TikTakToe backend.

---

## ASP.NET Core

| Variable                  | Default           | Description                                          |
| ------------------------- | ----------------- | ---------------------------------------------------- |
| `ASPNETCORE_URLS`         | `http://+:8080`   | Listening address(es) for the HTTP server.           |
| `ASPNETCORE_ENVIRONMENT`  | `Production`      | Runtime environment (`Development` / `Production`).  |

Setting `ASPNETCORE_ENVIRONMENT=Development` automatically enables:
- The Scalar/OpenAPI documentation UI.
- Automatic EF Core migration on startup.

---

## PostgreSQL

The backend reads the database connection from individual environment variables (preferred in containerised deployments) or from the `ConnectionStrings:DefaultConnection` app setting.

**Individual environment variables (Docker / production)**

| Variable       | Default       | Description                            |
| -------------- | ------------- | -------------------------------------- |
| `PGHOST`       | `localhost`   | PostgreSQL server hostname.            |
| `PGPORT`       | `5432`        | PostgreSQL server port.                |
| `PGDATABASE`   | `tiktaktoe`   | Database name.                         |
| `PGUSER`       | `app_user`    | Database user.                         |
| `PGPASSWORD`   | *(required)*  | Database password. **Never hardcode this value.** Set it via a secret or the `POSTGRES_PASSWORD` shell variable. |

**Connection string (local development without Docker)**

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tiktaktoe;Username=app_user"
  }
}
```

The individual `PG*` variables take precedence over the connection string when both are present.

---

## Feature flags

| Variable / App setting                     | Default     | Description                                                           |
| ------------------------------------------ | ----------- | --------------------------------------------------------------------- |
| `FEATURES__EXPOSEAPIDOCS`                  | `false`     | Expose the OpenAPI JSON spec and Scalar UI on non-development builds. |
| `FEATURES__APPLYMIGRATIONSONSTARTUP`       | `false`     | Run EF Core migrations automatically on startup.                      |

Both flags are forced to `true` when `ASPNETCORE_ENVIRONMENT=Development`.

**In `appsettings.json`:**

```json
{
  "Features": {
    "ExposeApiDocs": false,
    "ApplyMigrationsOnStartup": false
  }
}
```

---

## Docker Compose variables

The `docker-compose.yml` file accepts the following shell variables to override port bindings and credentials without editing the file.

| Shell variable        | Default      | Description                                   |
| --------------------- | ------------ | --------------------------------------------- |
| `POSTGRES_PASSWORD`   | `changeme-dev-only` | PostgreSQL superuser password. **Always override this in real deployments.** |
| `POSTGRES_PORT`       | `5432`       | Host port mapped to the PostgreSQL container. |
| `BACKEND_PORT`        | `8080`       | Host port mapped to the backend container.    |
| `FRONTEND_PORT`       | `3000`       | Host port mapped to the production frontend.  |
| `FRONTEND_DEV_PORT`   | `5173`       | Host port mapped to the Vite dev server.      |
| `DB_EXPLORER_PORT`    | `8081`       | Host port mapped to the pgweb UI.             |

Example — start the dev stack on non-default ports:

```bash
export POSTGRES_PASSWORD=mysecret
export BACKEND_PORT=9090
export FRONTEND_DEV_PORT=4000
docker compose --profile dev up --build
```
