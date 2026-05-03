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


