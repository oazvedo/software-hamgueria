import type { Metric } from 'web-vitals'

// Browser telemetry (spec 023): a W3C traceparent on every API call, so the API's traces carry the browser's trace id;
// JavaScript errors and Web Vitals posted to the API, which turns them into logs and metrics (ADR 0028).

const hex = (bytes: number) =>
  Array.from(crypto.getRandomValues(new Uint8Array(bytes)), (b) => b.toString(16).padStart(2, '0')).join('')

/** A fresh W3C trace context header: version 00, random trace and span ids, sampled. */
export function newTraceparent(): string {
  return `00-${hex(16)}-${hex(8)}-01`
}

function post(path: string, body: unknown): void {
  const json = JSON.stringify(body)
  // sendBeacon survives the page closing; fall back to fetch where it is missing or refuses.
  const sent = typeof navigator !== 'undefined' && navigator.sendBeacon?.(path, new Blob([json], { type: 'application/json' }))
  if (!sent) void fetch(path, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: json, keepalive: true }).catch(() => {})
}

const MAX_ERRORS_PER_PAGE = 5
let errorsSent = 0

export function reportError(error: unknown): void {
  if (errorsSent >= MAX_ERRORS_PER_PAGE) return
  errorsSent++
  const err = error instanceof Error ? error : new Error(String(error))
  post('/api/telemetry/client-error', {
    message: err.message.slice(0, 500) || 'Erro sem mensagem',
    stack: err.stack?.slice(0, 2000),
    page: window.location.href,
  })
}

export function reportVital(metric: Pick<Metric, 'name' | 'value' | 'rating'>): void {
  post('/api/telemetry/vitals', { name: metric.name, value: metric.value, rating: metric.rating, page: window.location.href })
}

let started = false

/** Starts error capture and Web Vitals once per page load. */
export async function startBrowserTelemetry(): Promise<void> {
  if (started || typeof window === 'undefined') return
  started = true
  window.addEventListener('error', (e) => reportError(e.error ?? e.message))
  window.addEventListener('unhandledrejection', (e) => reportError(e.reason))
  const { onCLS, onINP, onLCP, onFCP, onTTFB } = await import('web-vitals')
  for (const on of [onCLS, onINP, onLCP, onFCP, onTTFB]) on(reportVital)
}

/** For tests: forget what was already reported. */
export function resetBrowserTelemetry(): void {
  errorsSent = 0
}
