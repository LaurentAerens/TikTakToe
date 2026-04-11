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
│   └── engines.md                  # Engine architecture, interfaces & guide
└── src/
    └── backend/                    # Backend workspace
        ├── Dockerfile              # Backend container build definition
        ├── TikTakToe.slnx          # Solution file
        ├── TikTakToe/              # Main application project
        │   ├── Program.cs          # Entry point & DI configuration
        │   ├── Endpoints/          # Minimal-API endpoint definitions
        │   ├── Models/             # Data models / DTOs
        │   ├── Services/           # Business-logic services
        │   └── Engines/            # AI engine implementations
        │       ├── Interface/      # IEngine contract
        │       ├── Evaluation/     # Board evaluator implementations
        │       ├── Search/         # Opponent strategy implementations
        │       └── Exceptions/     # Engine-specific exceptions
        ├── TikTakToe.Console/      # Console test harness
        └── TikTakToe.Tests/        # xUnit test project
            └── engines/            # Engine contract & behaviour tests
```

---

## 🧰 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or the version used in this project)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerised runs)
- Optional: `curl` / `httpie` for manual endpoint testing

---

## 🧱 Build & Run Locally

### Without Docker

```bash
cd src/backend
dotnet restore
dotnet build --configuration Release
dotnet run --project TikTakToe
```

### With Docker Compose

```bash
export POSTGRES_PASSWORD=replace-with-a-strong-dev-password
docker compose up -d --build
```

The API will be available at **http://localhost:8080**.

The database explorer (pgweb) will be available at **http://localhost:8081** by default.
It auto-connects to the local PostgreSQL container using `POSTGRES_PASSWORD` (or `changeme-dev-only`).

If `POSTGRES_PASSWORD` is not set, Compose defaults to `changeme-dev-only` for local development.

OpenAPI and Scalar documentation endpoints are exposed only in development or when `Features:ExposeApiDocs=true`.

Development defaults are provided via `docker-compose.override.yml`, which Docker Compose applies automatically.

The `frontend` compose service is currently a placeholder container until the frontend app and its Dockerfile are added.

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

| Method | Endpoint   | Description          |
|--------|------------|----------------------|
| `GET`  | `/healthz` | Health check         |
| `GET`  | `/version` | Application version  |

---

## ⚙️ Configuration

| Environment Variable     | Default | Description                        |
|--------------------------|---------|------------------------------------|
| `ASPNETCORE_URLS`        | `http://+:8080` | Listening address          |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment           |

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
