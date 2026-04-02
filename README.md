# TikTakToe

> **Template repository** — rename all occurrences of `TikTakToe` (and the `src/backend/TikTakToe` directories) to match your actual project name after creating a repo from this template.

A short, one-paragraph description of what this project does and the problem it solves.

---

## 🚀 Features

- ✅ **Feature one** – describe it briefly.
- ✅ **Feature two** – describe it briefly.
- ✅ **Feature three** – describe it briefly.

---

## 📁 Project Structure

```
.
├── docker-compose.yml              # Local development compose file
├── docs/                           # Extended documentation
└── src/
    └── backend/                    # Backend workspace
        ├── Dockerfile              # Backend container build definition
        ├── TikTakToe.slnx          # Solution file
        ├── TikTakToe/              # Main application project
        │   ├── Program.cs          # Entry point & DI configuration
        │   ├── Endpoints/          # Minimal-API endpoint definitions
        │   ├── Models/             # Data models / DTOs
        │   └── Services/           # Business-logic services
        └── TikTakToe.Tests/        # xUnit test project
            └── ExampleServiceTests.cs  # Example test class
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
docker compose up -d --build
```

The API will be available at **http://localhost:8080**.

The `frontend` compose service is currently a placeholder container until the frontend app and its Dockerfile are added.

---

## 🧪 Running Tests

```bash
cd src/backend
dotnet test --configuration Release --verbosity normal
```

---

## 🌐 API Endpoints

| Method | Endpoint   | Description          |
|--------|------------|----------------------|
| `GET`  | `/healthz` | Health check         |
| `GET`  | `/version` | Application version  |

> Replace this table with your actual endpoints.

---

## ⚙️ Configuration

| Environment Variable     | Default | Description                        |
|--------------------------|---------|------------------------------------|
| `ASPNETCORE_URLS`        | `http://+:8080` | Listening address          |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment           |

> Add all relevant environment variables here.

---

## 📦 Docker

```bash
# Build
docker build -t TikTakToe:latest src/backend

# Run
docker run -p 8080:8080 TikTakToe:latest
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
