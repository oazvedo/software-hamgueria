import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { createOrder, updateOrderStatus } from '@/entities/order'
import { useAuthStore } from '@/features/auth'
import { ProductionPage } from '@/pages/production'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { renderWithProviders, server } from '@/shared/test'

const EMOJI = /\p{Extended_Pictographic}/u

beforeEach(() => {
  setToken('mock-kitchen-jwt')
  useAuthStore.setState({ token: 'mock-kitchen-jwt', isAuthenticated: true })
})
afterEach(() => {
  clearToken()
  useAuthStore.setState({ token: null, isAuthenticated: false })
})

describe('tela da cozinha', () => {
  it('006/CA-07, CA-05: dado em fonte de terminal com algarismos alinhados, nome e observação na fonte normal', async () => {
    await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 2, notes: 'sem cebola' }] })

    renderWithProviders(<ProductionPage />)

    const dish = await screen.findByText(/Burger Monolito/)
    const quantity = screen.getByText('2×')
    expect(quantity.className).toContain('font-mono')
    expect(quantity.className).toContain('tabular-nums')
    expect(dish.closest('[class*="font-mono"]')).toBeNull()

    const note = screen.getByText('sem cebola')
    expect(note.closest('[class*="font-mono"]')).toBeNull()
    expect(note.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true')
    expect(note.textContent).not.toMatch(/[↳→]/)
    expect(screen.getByText('Ana').closest('[class*="font-mono"]')).toBeNull()
  })

  it('006/CA-07: tempo de espera, id, total e contagens também usam a fonte de dados', async () => {
    await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 1 }] })
    const served = await createOrder({ tableId: 't2', customerName: 'Bia', items: [{ dishId: 'd4', quantity: 1 }] })
    await updateOrderStatus(served.id, 'Done')
    const isData = (el: Element) => {
      const data = el.closest('.font-mono')
      expect(data).not.toBeNull()
      expect(data!.className).toContain('tabular-nums')
    }

    renderWithProviders(<ProductionPage />)

    isData(await screen.findByText('agora há pouco')) // waiting time
    for (const count of screen.getAllByText('(1)')) isData(count) // "Em produção" and "Finalizados"
    isData(screen.getByText(/R\$\s*\d/)) // finished order total

    await userEvent.click(screen.getByRole('button', { name: 'Por mesa' }))
    isData(await screen.findByText(served.id.slice(0, 8))) // order/<id>
  })

  it('006/CA-08: com a fila vazia, a mensagem não tem emoji', async () => {
    renderWithProviders(<ProductionPage />)

    const empty = await screen.findByText('Nenhum pedido na fila. A cozinha está em dia.')
    expect(empty.textContent).not.toMatch(EMOJI)
  })

  it('006/CA-09: falha ao carregar mostra mensagem séria, sem travessão, dizendo o que fazer', async () => {
    server.use(http.get('/api/order', () => HttpResponse.json({ status: 500 }, { status: 500 })))

    renderWithProviders(<ProductionPage />)

    const error = await screen.findByText(
      'Não foi possível carregar os pedidos. Sua sessão pode ter expirado: saia e entre de novo.',
    )
    expect(error.textContent).not.toMatch(/—|\p{Extended_Pictographic}/u)
  })
})
