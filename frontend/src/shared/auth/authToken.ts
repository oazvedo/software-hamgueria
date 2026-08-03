// Single source of truth for the kitchen JWT, backed by localStorage so it
// survives reloads. Kept in `shared` so httpClient can read it without importing
// upward (FSD). The auth feature wraps these with reactive state.
const STORAGE_KEY = 'hamgueria-token'

let token: string | null = readInitial()

function readInitial(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY)
  } catch {
    return null
  }
}

export function getToken(): string | null {
  return token
}

export function setToken(value: string): void {
  token = value
  try {
    localStorage.setItem(STORAGE_KEY, value)
  } catch {
    /* ignore storage errors */
  }
}

export function clearToken(): void {
  token = null
  try {
    localStorage.removeItem(STORAGE_KEY)
  } catch {
    /* ignore storage errors */
  }
}
