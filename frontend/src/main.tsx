import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from '@/app/App'
import { enableMocking } from '@/shared/mocks'
import { startBrowserTelemetry } from '@/shared/telemetry'
import '@/app/styles/index.css'

void enableMocking().then(() => {
  void startBrowserTelemetry()
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  )
})
