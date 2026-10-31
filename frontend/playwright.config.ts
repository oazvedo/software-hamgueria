import { defineConfig, devices } from '@playwright/test'

// Two modes (ADR 0016): `mock` serves the app with MSW; `real` serves it without mocks against the API on :5262,
// which must be running on the `hamgueria_e2e` database (see CLAUDE.md). Only the selected mode's server starts.
const real = process.env.E2E_REAL === '1'
const port = real ? 5174 : 5173

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  reporter: 'list',
  use: { baseURL: `http://localhost:${port}`, trace: 'retain-on-failure' },
  projects: [
    { name: 'mock', use: { ...devices['Desktop Chrome'] } },
    { name: 'real', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: {
    command: `npm run dev -- --port ${port} --strictPort`,
    url: `http://localhost:${port}`,
    reuseExistingServer: !real,
    env: { VITE_ENABLE_MOCKS: real ? 'false' : 'true' },
  },
})
