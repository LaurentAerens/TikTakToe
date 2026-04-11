# =========================================
# Stage 1: Build the React (Vite) Application
# =========================================

ARG NODE_VERSION=24.14.1-alpine
FROM node:${NODE_VERSION} AS builder

WORKDIR /app

# Use the Yarn version expected by the repo
RUN corepack enable

# Copy dependency manifests first for better layer caching
COPY package.json yarn.lock ./

# Reproducible install for Yarn Berry
RUN --mount=type=cache,target=/root/.yarn/berry/cache \
    yarn install --immutable

# Copy the rest of the source
COPY . .

# Vite outputs to /app/dist
RUN yarn build

# =========================================
# Stage 2: Serve static files with Nginx
# =========================================
FROM nginx:stable-alpine AS runner

COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]