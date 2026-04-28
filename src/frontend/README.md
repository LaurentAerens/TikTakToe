# TikTakToe — Frontend

React + TypeScript + Vite frontend for the TikTakToe application.

---

## Prerequisites

- [Node.js](https://nodejs.org/) (LTS recommended)
- [Yarn](https://yarnpkg.com/) package manager
- A running TikTakToe backend (see the [root README](../../README.md) for setup instructions)

---

## Getting Started

Install dependencies:

```bash
yarn install
```

Start the development server with hot-module replacement:

```bash
yarn dev
```

The frontend will be available at **http://localhost:5173** and proxies API requests to the backend at **http://localhost:8080**.

---

## Available Scripts

| Command         | Description                                      |
| --------------- | ------------------------------------------------ |
| `yarn dev`      | Start the Vite development server with HMR.      |
| `yarn build`    | Compile and bundle for production.               |
| `yarn preview`  | Preview the production build locally.            |
| `yarn lint`     | Run ESLint across all source files.              |

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

---

## Running with Docker

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

