import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { THEME_STORAGE_KEY, followSystemTheme, useThemeStore } from '@/shared/theme'
import { ThemeToggle } from '@/shared/ui/molecules'

let systemListener: ((e: MediaQueryListEvent) => void) | undefined

beforeEach(() => {
  vi.stubGlobal('matchMedia', (query: string) => ({
    matches: false,
    media: query,
    addEventListener: (_: string, fn: (e: MediaQueryListEvent) => void) => (systemListener = fn),
    removeEventListener: () => (systemListener = undefined),
  }))
  useThemeStore.setState({ theme: 'dark', chosen: false })
  document.documentElement.classList.add('dark')
})
afterEach(() => {
  vi.unstubAllGlobals()
  localStorage.removeItem(THEME_STORAGE_KEY)
})

describe('tema claro e escuro (spec 014)', () => {
  it('014/CA-02: o botão alterna o tema, aplica na página e guarda a escolha', async () => {
    render(<ThemeToggle />)

    await userEvent.click(screen.getByRole('button', { name: 'Mudar para o tema claro' }))

    expect(document.documentElement.classList.contains('dark')).toBe(false)
    expect(document.documentElement.style.colorScheme).toBe('light')
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('light')
    expect(screen.getByRole('button', { name: 'Mudar para o tema escuro' })).toBeTruthy()

    await userEvent.click(screen.getByRole('button', { name: 'Mudar para o tema escuro' }))
    expect(document.documentElement.classList.contains('dark')).toBe(true)
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark')
  })

  it('014/CA-01: sem escolha, o tema acompanha o sistema', () => {
    const stop = followSystemTheme()

    systemListener!({ matches: true } as MediaQueryListEvent)

    expect(useThemeStore.getState().theme).toBe('light')
    expect(document.documentElement.classList.contains('dark')).toBe(false)
    stop()
  })

  it('014/CA-03: depois de escolher, mudar o sistema não troca o tema', async () => {
    const stop = followSystemTheme()
    render(<ThemeToggle />)
    await userEvent.click(screen.getByRole('button', { name: 'Mudar para o tema claro' }))

    systemListener!({ matches: false } as MediaQueryListEvent)

    expect(useThemeStore.getState().theme).toBe('light')
    stop()
  })
})
