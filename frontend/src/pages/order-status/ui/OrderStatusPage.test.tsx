import { describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import { createOrder } from '@/entities/order'
import { OrderStatusPage } from '@/pages/order-status'
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
})
