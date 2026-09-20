import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Прокси /api -> ASP.NET Core (localhost:5026) избавляет от проблем с CORS в dev-режиме.
// В проде прокси заменяется на nginx или настройку CORS на бэкенде.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5026',
        changeOrigin: true,
      },
    },
  },
})