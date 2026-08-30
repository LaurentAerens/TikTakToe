import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    watch: {
      usePolling: true,
    },
    // The backend has no CORS policy, so the browser must never call it directly.
    // Vite forwards /api itself, container-to-container, keeping every request same-origin.
    // `backend-dev` is the compose service name on tiktaktoe-network; override with
    // VITE_PROXY_TARGET=http://localhost:8080 when running `yarn dev` outside the container.
    proxy: {
      '/api': {
        target: process.env.VITE_PROXY_TARGET ?? 'http://backend-dev:8080',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})
