import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      // Proxies real API calls to the .NET backend during dev.
      // MSW intercepts first when enabled; set VITE_ENABLE_MOCKS=false to hit the real backend.
      // Backend runs on http://localhost:5262 (see API launchSettings); override with VITE_API_PROXY_TARGET.
      '/api': {
        target: process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5262',
        changeOrigin: true,
      },
    },
  },
})
