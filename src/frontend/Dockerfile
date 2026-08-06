# =========================================
# Stage: Shared dependency installation
# =========================================

ARG NODE_VERSION=24.14.1
FROM node:${NODE_VERSION} AS deps

WORKDIR /app

# Use the Yarn version expected by the repo
RUN corepack enable

# Copy dependency manifests first for better layer caching
COPY package.json yarn.lock ./

# Reproducible install for Yarn Berry
RUN --mount=type=cache,target=/root/.yarn/berry/cache \
    yarn install --immutable

# =========================================
# Stage: Development (Vite dev server, hot reload)
# =========================================
FROM deps AS dev

# Copy rest of the source code
COPY . .

# Ensure the "node" user owns the application files, and pre-create the
# VS Code Server dir so the devcontainer's named volume inherits node:node
# ownership on first mount instead of being created root-owned.
RUN chown -R node:node /app && \
    mkdir -p /home/node/.vscode-server && \
    chown -R node:node /home/node/.vscode-server

# Switch to the built-in non-root "node" user
USER node

# Expose Vite dev server port (matches FRONTEND_PORT, shared with the prod
# frontend service, the same way BACKEND_PORT is shared by backend/backend-dev)
EXPOSE 3000

# Run Vite in dev mode, accessible outside the container
CMD ["yarn", "dev", "--host", "0.0.0.0", "--port", "3000"]

# =========================================
# Stage: Build the React (Vite) production bundle
# =========================================
FROM deps AS builder

# Copy the rest of the source
COPY . .

# Vite outputs to /app/dist
RUN yarn build

# =========================================
# Stage: Serve static files with Nginx (production)
# =========================================
FROM nginx:stable-alpine AS prd

COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
