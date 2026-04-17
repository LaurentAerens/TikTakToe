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

### Backend

#### Without Docker

```bash
cd src/backend
dotnet restore
dotnet build --configuration Release
dotnet run --project TikTakToe
```

#### With Docker Compose

```bash
export POSTGRES_PASSWORD=replace-with-a-strong-dev-password
docker compose up -d --build
```

The API will be available at **http://localhost:8080**.

### Frontend

#### Development

```bash
cd src/frontend
docker compose up dev --watch
```

Frontend will be available on **http://localhost:5173**.

##### Using Yarn

```bash
cd src/frontend
yarn dev
```

##### Without compose

```bash
cd src/frontend
docker build -t tiktaktoe-frontend-dev -f Dockerfile.dev .
docker run -p 5173:5173 tiktaktoe-frontend-dev
```

#### Production

```bash
cd src/frontend
docker compose up prod
```

Frontend will be avalible on **http://localhost:3000**.

##### Without compose

```bash
cd src/frontend
docker build -t tiktaktoe-frontend -f Dockerfile .
docker run -p 3000:80 tiktaktoe-frontend
```

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
