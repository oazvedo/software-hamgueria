import { describe, expect, it } from 'vitest'
import type { OrderResponse } from '@/entities/order'
import { summarizeComandas } from '@/pages/table-comandas/model/summarizeComandas'

const order = (customerName: string, createdAt: string, over: Partial<OrderResponse> = {}): OrderResponse =>
  ({
    id: `${customerName}-${createdAt}`,
    tableId: 't1',
    tableName: 'Mesa 01',
    customerName,
    items: [{ dishId: 'd1', dishName: 'Burger', price: 10, quantity: 2, notes: '' }],
    status: 'Created',
    comandaClosed: false,
    createdAt,
    updatedAt: createdAt,
    ...over,
  }) as OrderResponse

describe('resumo das comandas da mesa', () => {
  it('007/CA-02: abertas primeiro, das mais recentes, com pedidos, em produção e total', () => {
    const result = summarizeComandas([
      order('Caio', '2026-10-31T10:00:00Z', { comandaClosed: true, status: 'Done' }),
      order('Ana', '2026-10-31T11:00:00Z'),
      order('Bia', '2026-10-31T12:00:00Z', { status: 'Done' }),
      order('Ana', '2026-10-31T11:30:00Z', { status: 'Done' }),
    ])

    expect(result.map((c) => c.customerName)).toEqual(['Bia', 'Ana', 'Caio'])
    expect(result[1]).toMatchObject({ orders: 2, inProduction: 1, total: 40, closed: false })
    expect(result[2].closed).toBe(true)
  })

  it('007/CA-05: mesa sem pedidos não tem comandas', () => {
    expect(summarizeComandas([])).toEqual([])
  })

  it('007/CA-02: ordena por instante, não por texto, com frações de segundo de tamanho diferente', () => {
    const result = summarizeComandas([
      order('Ana', '2026-10-31T12:00:00Z'),
      order('Bia', '2026-10-31T12:00:00.5Z'),
      order('Caio', '2026-10-31T12:00:00.123456Z'),
    ])

    expect(result.map((c) => c.customerName)).toEqual(['Bia', 'Caio', 'Ana'])
  })
})
