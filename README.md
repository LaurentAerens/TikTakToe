# TikTakToe

A .NET web API that plays Tic-Tac-Toe using a suite of pluggable AI engines. Each engine has a different strategy — from brute-force minimax to heuristic and randomised approaches — making it easy to compare opponent difficulty levels or build your own engine on top of the shared interface.

---

## 🚀 Features

- ✅ **Multiple AI engines** – classical minimax, heuristic half-depth, random, and opportunity-based engines out of the box.
- ✅ **Composable engine architecture** – mix and match board evaluators with opponent strategies to produce new engine behaviour.
- ✅ **Parallel root search** – the minimax base class evaluates root-level moves in parallel for faster response times.

---

## 📁 Project Structure

```
.
├── docker-compose.yml              # Local development compose file
├── docs/                           # Extended documentation
│   ├── api.md                      # Full API reference with request/response examples
│   ├── configuration.md            # Complete configuration reference
│   └── engines.md                  # Engine architecture, interfaces & guide
└── src/
    ├── backend/                    # Backend workspace
    │   ├── Dockerfile              # Backend container build definition
    │   ├── TikTakToe.slnx          # Solution file
    │   ├── TikTakToe/              # Main application project
    │   │   ├── Program.cs          # Entry point & DI configuration
    │   │   ├── Endpoints/          # Minimal-API endpoint definitions
    │   │   ├── Models/             # Data models / DTOs
    │   │   ├── Services/           # Business-logic services
    │   │   └── Engines/            # AI engine implementations
    │   │       ├── Interface/      # IEngine contract
    │   │       ├── Evaluation/     # Board evaluator implementations
    │   │       ├── Search/         # Opponent strategy implementations
    │   │       └── Exceptions/     # Engine-specific exceptions
    │   ├── TikTakToe.Console/      # Console test harness
    │   └── TikTakToe.Tests/        # xUnit test project
    │       └── engines/            # Engine contract & behaviour tests
    └── frontend/                   # Frontend workspace
        ├── src/                    # Code folder
        │   ├── assets/             # Images
        │   └── main.tsx            # Entry point
```

---

## 🧰 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or the version used in this project)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerised runs)
- Optional: `curl` / `httpie` for manual endpoint testing

---

## 🧱 Build & Run Locally

### With Docker (Recommended)

Set your database password before starting:

```bash
export POSTGRES_PASSWORD=replace-with-a-strong-dev-password
```

When switching between modes, clean up first to avoid stale containers:

```bash
docker compose down --remove-orphans
```

#### Development Mode

Starts the full development stack with hot-reload and debugging tools:

- Backend (Development mode with Scalar/OpenAPI)
- Frontend (Vite dev server with live updates)
- PostgreSQL database
- Database explorer (pgweb)

```bash
docker compose --profile dev up --build
```

**Available at:**
- Frontend: http://localhost:5173
- Backend API: http://localhost:8080
- Database Explorer: http://localhost:8081

#### Test Mode

Starts only the backend and database for black-box testing:

- Backend (Production mode)
- PostgreSQL database

```bash
docker compose --profile test up --build
```

**Available at:**
- Backend API: http://localhost:8080

#### Production Mode

Starts the full production stack:

- Backend (Production mode)
- Frontend (optimized build)
- PostgreSQL database

```bash
docker compose --profile prd up --build
```

**Available at:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080

### Without Docker

#### Backend

```bash
cd src/backend
dotnet restore
dotnet build --configuration Release
dotnet run --project TikTakToe
```

Backend API will be available at **http://localhost:8080**.

#### Frontend

Development server:

```bash
cd src/frontend
yarn install
yarn dev
```

Frontend will be available at **http://localhost:5173**.

---

## 🧪 Running Tests

```bash
cd src/backend
dotnet test --configuration Release --verbosity normal
```

---

## 🤖 Engines

See [docs/engines.md](docs/engines.md) for a full reference of the available AI engines, the `IEngine` interface, how board evaluation and opponent strategies work, and a step-by-step guide to creating your own engine.

---

## 🌐 API Endpoints

| Method  | Endpoint                                     | Description                                  |
|---------|----------------------------------------------|----------------------------------------------|
| `GET`   | `/healthz`                                   | Health check                                 |
| `GET`   | `/version`                                   | Application version                          |
| `POST`  | `/games`                                     | Create a new game                            |
| `GET`   | `/games/{id}`                                | Get a game by ID                             |
| `GET`   | `/engines`                                   | List all engine capabilities                 |
| `GET`   | `/engines/{id}`                              | Get engine details by ID                     |
| `GET`   | `/engines/{displayName}`                     | Get engine details by display name           |
| `GET`   | `/engines/resolve-engine-id/{playerId}`      | Convert engine player ID to engine ID        |

See [docs/api.md](docs/api.md) for request/response shapes and worked examples.

---

## ⚙️ Configuration

| Environment Variable              | Default           | Description                                          |
|-----------------------------------|-------------------|------------------------------------------------------|
| `ASPNETCORE_URLS`                 | `http://+:8080`   | Listening address                                    |
| `ASPNETCORE_ENVIRONMENT`          | `Production`      | Runtime environment (`Development` / `Production`)   |
| `PGHOST`                          | `localhost`       | PostgreSQL host                                      |
| `PGPORT`                          | `5432`            | PostgreSQL port                                      |
| `PGDATABASE`                      | `tiktaktoe`       | Database name                                        |
| `PGUSER`                          | `app_user`        | Database user                                        |
| `PGPASSWORD`                      | *(required)*      | Database password                                    |
| `FEATURES__EXPOSEAPIDOCS`         | `false`           | Expose OpenAPI/Scalar UI outside Development mode    |
| `FEATURES__APPLYMIGRATIONSONSTARTUP` | `false`        | Run EF Core migrations automatically on startup      |

See [docs/configuration.md](docs/configuration.md) for the full configuration reference including Docker Compose shell variables.

---

## 📦 Docker

```bash
# Build
docker build -t tiktaktoe:latest src/backend

# Run
docker run -p 8080:8080 tiktaktoe:latest
```

---

## 🔐 Security

See [SECURITY.md](SECURITY.md) for the supported version table and vulnerability reporting instructions.

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on bug reports, feature requests, and pull requests.

---

## 📄 License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
