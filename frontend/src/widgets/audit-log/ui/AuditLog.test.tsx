import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { screen, within } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { createOrder, updateOrderStatus } from '@/entities/order'
import { AuditLog } from '@/widgets/audit-log'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { renderWithProviders, server } from '@/shared/test'

beforeEach(() => setToken('mock-kitchen-jwt'))
afterEach(() => clearToken())

describe('histórico de mudanças', () => {
  it('009/CA-07: mostra quem criou e quem alterou, com antes e depois e a tela de origem', async () => {
    const order = await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 1 }] })
    await updateOrderStatus(order.id, 'Done')

    renderWithProviders(<AuditLog />)

    await screen.findByText(/admin/)
    const list = screen.getByRole('list', { name: 'Mudanças' })
    const rows = [...list.children] as HTMLElement[]
    expect(rows[0].textContent).toMatch(/admin alterou pedido/)
    expect(within(rows[0]).getByText('Status').parentElement!.textContent).toMatch(/de\s*Created\s*para\s*Done/)
    expect(rows[0].textContent).toContain('/producao')
    expect(rows[1].textContent).toMatch(/Ana criou pedido/)
    expect(rows[1].textContent).toContain('/menu')
  })

  it('009/CA-07: sem mudanças, avisa que não há histórico', async () => {
    renderWithProviders(<AuditLog />)

    expect(await screen.findByText('Nenhuma mudança registrada ainda.')).toBeTruthy()
  })

  it('009/CA-05 (falha): sessão expirada mostra o motivo sem detalhe técnico', async () => {
    server.use(http.get('/api/audit', () => HttpResponse.json({ status: 401 }, { status: 401 })))

    renderWithProviders(<AuditLog />)

    expect(await screen.findByText(/Não foi possível carregar o histórico/)).toBeTruthy()
  })
})
