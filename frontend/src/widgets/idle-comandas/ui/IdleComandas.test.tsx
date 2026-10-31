import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { IdleComandas } from '@/widgets/idle-comandas'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { renderWithProviders } from '@/shared/test'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

const at = (hoursAgo: number) => new Date(Date.now() - hoursAgo * 3_600_000).toISOString()
const order = (id: string, customerName: string, hoursAgo: number) => ({
  id, tableId: 't1', tableName: 'Mesa 01', customerName, items: [], status: 'Done', createdAt: at(hoursAgo), updatedAt: at(hoursAgo),
})

beforeEach(() => setToken('mock-kitchen-jwt'))
afterEach(() => clearToken())

describe('comandas esquecidas (spec 025)', () => {
  it('025/CA-01, CA-04: avisa a comanda parada há mais de 6 h e fecha só quando a cozinha pede', async () => {
    localStorage.setItem('hamgueria-mock-orders', JSON.stringify([order('a', 'Ana', 30), order('b', 'Bia', 1)]))

    renderWithProviders(<IdleComandas />)

    const section = await screen.findByRole('region', { name: /comanda aberta sem pedido há mais de 6 h/ })
    expect(within(section).getByText('Ana')).toBeTruthy()
    expect(within(section).queryByText('Bia')).toBeNull()
    expect(section.textContent).toContain('há 30 h')

    await userEvent.click(within(section).getByRole('button', { name: 'Fechar comanda' }))

    await vi.waitFor(() => expect(screen.queryByRole('region', { name: /sem pedido/ })).toBeNull())
  })

  it('025/CA-02: sem comanda parada, nenhum aviso', async () => {
    localStorage.setItem('hamgueria-mock-orders', JSON.stringify([order('b', 'Bia', 1)]))

    renderWithProviders(<IdleComandas />)

    await new Promise((r) => setTimeout(r, 50))
    expect(screen.queryByRole('region')).toBeNull()
  })
})
