import { describe, expect, it, vi } from 'vitest'
import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createOrder, getOrder, updateOrderStatus } from '@/entities/order'
import { OrderStatusPage } from '@/pages/order-status'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { renderWithProviders } from '@/shared/test'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

const renderAt = (id: string) => renderWithProviders(<OrderStatusPage />, { route: `/order/${id}`, path: '/order/:id' })
const newOrder = () =>
  createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 2 }, { dishId: 'd2', quantity: 1 }] })

describe('cliente muda o próprio pedido (spec 015)', () => {
  it('015/CA-01: no aparelho que fez o pedido, cancelar pede confirmação e cancela', async () => {
    const order = await newOrder()
    renderAt(order.id)

    await userEvent.click(await screen.findByRole('button', { name: 'Cancelar pedido' }))
    const dialog = screen.getByRole('dialog')
    expect(within(dialog).getByText('A cozinha deixa de prepará-lo. Não dá para desfazer.')).toBeTruthy()
    await userEvent.click(within(dialog).getByRole('button', { name: 'Cancelar pedido' }))

    await vi.waitFor(async () => expect((await getOrder(order.id)).status).toBe('Cancelled'))
    expect(await screen.findAllByText('Cancelado')).toBeTruthy()
  })

  it('015/CA-02: alterar itens muda as quantidades e zerar um item o tira do pedido', async () => {
    const order = await newOrder()
    renderAt(order.id)

    await userEvent.click(await screen.findByRole('button', { name: 'Alterar itens' }))
    const dialog = screen.getByRole('dialog')
    await userEvent.click(within(dialog).getByRole('button', { name: 'Mais um Burger Monolito' }))
    await userEvent.click(within(dialog).getByRole('button', { name: 'Tirar um Isca de Júnior Frita' }))
    await userEvent.click(within(dialog).getByRole('button', { name: 'Salvar alteração' }))

    await vi.waitFor(async () => {
      const items = (await getOrder(order.id)).items
      expect(items.map((i) => [i.dishName, i.quantity])).toEqual([['Burger Monolito', 3]])
    })
  })

  it('015/CA-03 (falha): zerar todos os itens não deixa salvar e diz o motivo', async () => {
    const order = await newOrder()
    renderAt(order.id)

    await userEvent.click(await screen.findByRole('button', { name: 'Alterar itens' }))
    const dialog = screen.getByRole('dialog')
    await userEvent.click(within(dialog).getByRole('button', { name: 'Tirar um Isca de Júnior Frita' }))
    await userEvent.click(within(dialog).getByRole('button', { name: 'Tirar um Burger Monolito' }))
    await userEvent.click(within(dialog).getByRole('button', { name: 'Tirar um Burger Monolito' }))

    expect(within(dialog).getByRole('alert').textContent).toContain('pelo menos um item')
    expect((within(dialog).getByRole('button', { name: 'Salvar alteração' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('015/CA-04 (falha): em outro aparelho (sem o token), os botões não aparecem', async () => {
    const order = await newOrder()
    localStorage.removeItem('hamgueria-owner-tokens')
    renderAt(order.id)

    await screen.findByRole('heading', { name: 'Acompanhando seu pedido' })
    expect(screen.queryByRole('button', { name: 'Cancelar pedido' })).toBeNull()
    expect(screen.queryByRole('button', { name: 'Alterar itens' })).toBeNull()
  })

  it('015/CA-05 (falha): pedido servido não oferece alterar nem cancelar', async () => {
    const order = await newOrder()
    setToken('mock-kitchen-jwt')
    await updateOrderStatus(order.id, 'Done')
    clearToken()
    renderAt(order.id)

    await screen.findByText('Pedido servido. Deploy em produção concluído, bom apetite!')
    expect(screen.queryByRole('button', { name: 'Cancelar pedido' })).toBeNull()
  })
})
