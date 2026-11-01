import { describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import { createOrder, updateOrderStatus } from '@/entities/order'
import { OrderStatusPage } from '@/pages/order-status'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { renderWithProviders } from '@/shared/test'

const renderAt = (id: string) => renderWithProviders(<OrderStatusPage />, { route: `/order/${id}`, path: '/order/:id' })

describe('acompanhamento do pedido', () => {
  it('001/CA-06: a tela mostra o pedido criado no estágio "em produção"', async () => {
    const order = await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 2 }] })

    renderAt(order.id)

    expect(await screen.findByRole('heading', { name: 'Acompanhando seu pedido' })).toBeTruthy()
    expect(screen.getByText('Burger Monolito')).toBeTruthy()
    expect(screen.getByText('monitorando ao vivo')).toBeTruthy()
    expect(screen.getAllByText('Em produção').length).toBeGreaterThan(0)
  })

  it('001/CA-06: pedido inexistente mostra "Pedido não encontrado"', async () => {
    renderAt('nao-existe')

    expect(await screen.findByText('Pedido não encontrado')).toBeTruthy()
  })

  it('006/CA-05, CA-08: pedido servido mostra a observação com ícone oculto e a mensagem sem travessão nem emoji', async () => {
    const order = await createOrder({
      tableId: 't1',
      customerName: 'Ana',
      items: [{ dishId: 'd4', quantity: 1, notes: 'sem cebola' }],
    })
    setToken('mock-kitchen-jwt') // the kitchen serves it
    await updateOrderStatus(order.id, 'Done')
    clearToken()

    renderAt(order.id)

    const done = await screen.findByText('Pedido servido. Deploy em produção concluído, bom apetite!')
    expect(done.textContent).not.toMatch(/—|\p{Extended_Pictographic}/u)
    expect(screen.getByText('sem cebola').querySelector('svg')?.getAttribute('aria-hidden')).toBe('true')
  })
})
