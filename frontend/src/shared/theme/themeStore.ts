import { create } from 'zustand'

// Light and dark themes (spec 014). The choice is a plain string in localStorage (also read by the inline script in
// index.html, before the first paint); without a choice, the system's preference wins and is followed live.

export type Theme = 'light' | 'dark'
export const THEME_STORAGE_KEY = 'hamgueria-theme'

const darkQuery = () =>
  typeof window !== 'undefined' && typeof window.matchMedia === 'function'
    ? window.matchMedia('(prefers-color-scheme: dark)')
    : undefined

function storedTheme(): Theme | undefined {
  try {
    const value = localStorage.getItem(THEME_STORAGE_KEY)
    return value === 'light' || value === 'dark' ? value : undefined
  } catch {
    return undefined
  }
}

const systemTheme = (): Theme => (darkQuery()?.matches ?? true ? 'dark' : 'light')

export function applyTheme(theme: Theme): void {
  const root = document.documentElement
  root.classList.toggle('dark', theme === 'dark')
  root.style.colorScheme = theme
}

interface ThemeState {
  theme: Theme
  /** True once the person picked a theme; until then the system decides. */
  chosen: boolean
  toggle: () => void
}

export const useThemeStore = create<ThemeState>((set, get) => ({
  theme: storedTheme() ?? systemTheme(),
  chosen: storedTheme() !== undefined,
  toggle: () => {
    const theme: Theme = get().theme === 'dark' ? 'light' : 'dark'
    try {
      localStorage.setItem(THEME_STORAGE_KEY, theme)
    } catch {
      // Private mode: the toggle still works for this visit.
    }
    applyTheme(theme)
    set({ theme, chosen: true })
  },
}))

/** Follows the system theme while the person has not picked one. Returns the unsubscribe. */
export function followSystemTheme(): () => void {
  const query = darkQuery()
  if (!query) return () => {}
  const onChange = (e: MediaQueryListEvent) => {
    if (useThemeStore.getState().chosen) return
    const theme: Theme = e.matches ? 'dark' : 'light'
    applyTheme(theme)
    useThemeStore.setState({ theme })
  }
  query.addEventListener('change', onChange)
  return () => query.removeEventListener('change', onChange)
}
