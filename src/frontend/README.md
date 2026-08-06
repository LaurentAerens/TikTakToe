# TikTakToe — Frontend

React + TypeScript + Vite frontend for the TikTakToe application.

---

## Prerequisites

- [Node.js](https://nodejs.org/) (24.12.0 LTS)
- [Yarn](https://yarnpkg.com/)
- A running TikTakToe backend (see the [root README](../../README.md) for setup instructions)

---

## Running with Docker

It's recomended to run via docker to prevent dependency issues.

The frontend is included in the Docker Compose dev and production profiles.

Development (Vite dev server with hot-reload):

```bash
docker compose --profile dev up --build
```

Production (optimized static build served by Nginx):

```bash
docker compose --profile prd up --build
```

See the [root README](../../README.md) for full Docker usage instructions.

---

## Dev Containers (VS Code)

Instead of running Docker Compose manually and editing on the host, you can develop directly inside the `frontend-dev` container — TypeScript/ESLint IntelliSense then runs against the container's own installed dependencies, with no local Node/Yarn install needed.

1. Install the [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) VS Code extension.
2. From the repo root: Command Palette → **Dev Containers: Reopen in Container** (pick "TikTakToe Frontend"). Or open this `frontend` folder directly and use **Dev Containers: Open Folder in Container...**.
3. This starts `frontend-dev` plus `backend-dev`, `postgres`, and `db-explorer` in the background, forwarding port 3000 (frontend) and 8080 (backend API).

Config lives in [.devcontainer/](.devcontainer/) — it reuses the same [Dockerfile](Dockerfile) (`dev` target) and the root [docker-compose.yml](../../docker-compose.yml) rather than a separate build definition.

---

## Local development

Install dependencies:

```bash
yarn install
```

Use the docker to run.

---

## Project Structure

```
src/
├── assets/         # Static images and icons
├── App.tsx         # Root application component
├── App.css         # Root component styles
├── index.css       # Global styles
└── main.tsx        # Entry point — mounts the React app
```


