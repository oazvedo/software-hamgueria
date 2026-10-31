import { beforeEach, describe, expect, it } from 'vitest'
import { screen, within } from '@testing-library/react'
import { closeComanda, createOrder, useOrderStore } from '@/entities/order'
import { TableComandasPage } from '@/pages/table-comandas'
import { AppRoutes } from '@/app/router/routes'
import { renderWithProviders } from '@/shared/test'
import { mockDishes } from '@/shared/mocks/data'
import { formatCurrency } from '@/shared/lib/formatCurrency'

const renderList = (route = '/comandas/t1') =>
  renderWithProviders(<TableComandasPage />, { route, path: '/comandas/:tableId' })

beforeEach(() => useOrderStore.setState({ tableId: null, customerName: '', items: [] }))

describe('comandas da mesa', () => {
  it('007/CA-02, CA-04: lista todas as comandas, abertas primeiro, com estado, pedidos e total', async () => {
    await createOrder({ tableId: 't1', customerName: 'Caio', items: [{ dishId: 'd4', quantity: 1 }] })
    await closeComanda('t1', 'Caio')
    await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 1 }] })
    await createOrder({ tableId: 't1', customerName: 'Bia', items: [{ dishId: 'd4', quantity: 2 }] })

    renderList()

    const list = await screen.findByRole('list', { name: 'Comandas da mesa' })
    const rows = within(list).getAllByRole('link')
    expect(rows.map((r) => r.textContent?.match(/^(Ana|Bia|Caio)/)?.[0])).toEqual(['Bia', 'Ana', 'Caio'])
    expect(rows[2].textContent).toContain('Fechada')
    expect(rows[0].textContent).toContain('Aberta')
    expect(rows[0].getAttribute('href')).toBe('/comanda/t1/Bia')
    expect(screen.getByText('abertas', { exact: false }).textContent).toMatch(/3\s*comandas,\s*2\s*abertas/)

    // each row: orders, in production and total; and the table total (RN-02, RN-05)
    const price = mockDishes.find((d) => d.id === 'd4')!.price
    expect(rows[0].textContent).toMatch(/1\s*pedido/)
    expect(rows[0].textContent).toMatch(/1\s*em produção/)
    expect(rows[0].textContent).toContain(formatCurrency(price * 2))
    expect(rows[1].textContent).toContain(formatCurrency(price))
    expect(rows[2].textContent).toMatch(/1\s*pedido/)
    expect(screen.getByText('abertas', { exact: false }).parentElement!.textContent).toContain(formatCurrency(price * 4))
  })

  it('007/CA-03: a comanda do cliente aparece marcada', async () => {
    await createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 1 }] })
    await createOrder({ tableId: 't1', customerName: 'Bia', items: [{ dishId: 'd4', quantity: 1 }] })
    useOrderStore.setState({ tableId: 't1', customerName: 'Ana' })

    renderList()

    const ana = (await screen.findByText('Ana')).closest('a')!
    expect(within(ana).getByText('sua comanda')).toBeTruthy()
    expect(within(screen.getByText('Bia').closest('a')!).queryByText('sua comanda')).toBeNull()
  })

  it('007/CA-05: mesa sem pedidos mostra o aviso e o caminho para o cardápio', async () => {
    renderList('/comandas/t9')

    expect(await screen.findByText('Nenhuma comanda nesta mesa ainda.')).toBeTruthy()
    expect(screen.getByRole('link', { name: /Ir para o cardápio/ }).getAttribute('href')).toBe('/menu')
  })

  it('007/CA-06: o endereço antigo da mesa leva à lista de comandas', async () => {
    renderWithProviders(<AppRoutes />, { route: '/mesa/t9' })

    expect(await screen.findByText('Nenhuma comanda nesta mesa ainda.')).toBeTruthy()
    expect(screen.getByText('Comandas da mesa')).toBeTruthy()
  })
})
