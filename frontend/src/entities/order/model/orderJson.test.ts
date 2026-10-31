import { describe, expect, it } from 'vitest'
import { buildOrderJson, buildOrderPayload } from '@/entities/order/model/orderJson'
import type { OrderItem } from '@/entities/order/model/types'

const item = (over: Partial<OrderItem> = {}): OrderItem => ({
  dishId: 'd4', name: 'Burger Monolito', category: 'Hamburgers', price: 34.9, quantity: 2, notes: '', ...over,
})

describe('JSON do pedido', () => {
  it('001/CA-02: o JSON mostra comanda, itens, observação e total', () => {
    const json = buildOrderJson({ tableId: 't1', customerName: 'Ana', items: [item({ notes: ' sem cebola ' })] })

    expect(json).toEqual({
      comanda: 'Ana',
      tableId: 't1',
      items: [{ dishId: 'd4', dish: 'Burger Monolito', category: 'Hamburgers', quantity: 2, unitPrice: 34.9, obs: 'sem cebola' }],
      total: 69.8,
    })
  })

  it('001/CA-03: o payload enviado tem só o que o servidor espera, com o nome sem espaços', () => {
    const payload = buildOrderPayload({ tableId: 't1', customerName: '  Ana ', items: [item()] })

    expect(payload).toEqual({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 2, notes: undefined }] })
  })

  it('001/CA-04: pedido sem nome de comanda não é montado', () => {
    expect(() => buildOrderPayload({ tableId: 't1', customerName: ' ', items: [item()] })).toThrow(/nome da comanda/)
  })

  it('001/CA-04: pedido sem mesa não é montado', () => {
    expect(() => buildOrderPayload({ tableId: null, customerName: 'Ana', items: [item()] })).toThrow(/mesa/)
  })
})
