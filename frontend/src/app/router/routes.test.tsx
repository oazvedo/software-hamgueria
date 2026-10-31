import { afterEach, describe, expect, it, vi } from 'vitest'
import { lazy, Suspense } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AppRoutes, ChunkErrorBoundary } from '@/app/router/routes'
import { renderWithProviders } from '@/shared/test'

afterEach(() => vi.restoreAllMocks())

describe('rotas sob demanda (spec 021)', () => {
  it('021/CA-02: enquanto a tela baixa aparece o indicador, e depois a tela', async () => {
    renderWithProviders(<AppRoutes />, { route: '/' })

    expect(screen.getByRole('status', { name: 'Carregando a página' })).toBeTruthy()
    expect(await screen.findByRole('link', { name: /Abrir cardápio/ })).toBeTruthy()
    expect(screen.queryByRole('status', { name: 'Carregando a página' })).toBeNull()
  })

  it('021/CA-02 (falha): se a tela não baixa, aparece o aviso com Recarregar em vez de tela em branco', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {}) // React logs the caught error
    const reload = vi.fn()
    vi.spyOn(window, 'location', 'get').mockReturnValue({ ...window.location, reload })
    const Broken = lazy(() => Promise.reject(new Error('Failed to fetch dynamically imported module')))

    render(
      <ChunkErrorBoundary>
        <Suspense fallback={null}>
          <Broken />
        </Suspense>
      </ChunkErrorBoundary>,
    )

    expect((await screen.findByRole('alert')).textContent).toContain('Não foi possível carregar esta tela.')
    await userEvent.click(screen.getByRole('button', { name: 'Recarregar' }))
    expect(reload).toHaveBeenCalled()
  })
})
