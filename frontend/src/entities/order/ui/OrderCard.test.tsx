import { describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import { OrderCard } from '@/entities/order'
import type { OrderResponse } from '@/entities/order/api/orderApi'
import { renderWithProviders } from '@/shared/test'

const order: OrderResponse = {
  id: 'abcdef12-0000-0000-0000-000000000000',
  tableId: 't1',
  tableName: 'Mesa 1',
  customerName: 'Ana',
  items: [{ dishId: 'd4', dishName: 'Burger Monolito', price: 30, quantity: 2, notes: 'sem cebola' }],
  status: 'Created',
  comandaClosed: false,
  createdAt: '2026-10-31T12:00:00Z',
  updatedAt: '2026-10-31T12:00:00Z',
} as OrderResponse

describe('cartão do pedido', () => {
  it('006/CA-05: a observação vem com um ícone que o leitor de tela não anuncia', () => {
    renderWithProviders(<OrderCard order={order} />)

    const note = screen.getByText('sem cebola')
    expect(note.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true')
    expect(note.textContent).toBe('sem cebola')
  })

  it('006/CA-06: o link diz "Ver detalhe", com ícone fora do leitor de tela e sem seta no texto', () => {
    renderWithProviders(<OrderCard order={order} />)

    const link = screen.getByRole('link', { name: 'Ver detalhe' })
    expect(link.getAttribute('href')).toBe(`/order/${order.id}`)
    expect(link.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true')
    expect(link.textContent).not.toMatch(/[→↳]/)
  })
})
