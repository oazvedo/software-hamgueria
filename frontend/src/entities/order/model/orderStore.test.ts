import { beforeEach, describe, expect, it } from 'vitest'
import { useOrderStore } from '@/entities/order/model/orderStore'
import { mockDishes } from '@/shared/mocks/data'

const [burger, chopp] = [mockDishes[3], mockDishes[10]]
const store = () => useOrderStore.getState()

beforeEach(() => useOrderStore.setState({ tableId: null, customerName: '', items: [] }))

describe('carrinho do pedido', () => {
  it('001/CA-02: adicionar o mesmo prato soma a quantidade em vez de duplicar', () => {
    store().addDish(burger)
    store().addDish(burger)
    store().addDish(chopp)

    expect(store().items.map((i) => [i.dishId, i.quantity])).toEqual([
      [burger.id, 2],
      [chopp.id, 1],
    ])
  })

  it('001/CA-02: mudar quantidade e observação atualiza o item', () => {
    store().addDish(burger)
    store().setQuantity(burger.id, 3)
    store().setNotes(burger.id, 'sem cebola')

    expect(store().items[0]).toMatchObject({ quantity: 3, notes: 'sem cebola' })
  })

  it('001/CA-02: quantidade zero remove o item', () => {
    store().addDish(burger)
    store().setQuantity(burger.id, 0)

    expect(store().items).toEqual([])
  })

  it('001/CA-03: limpar o carrinho mantém mesa e nome da comanda para o próximo pedido', () => {
    store().setTable('t1')
    store().setCustomerName('Ana')
    store().addDish(burger)

    store().clear()

    expect(store()).toMatchObject({ items: [], tableId: 't1', customerName: 'Ana' })
  })

  it('001/RN-06: trocar de mesa limpa o nome da comanda', () => {
    store().setTable('t1')
    store().setCustomerName('Ana')

    store().setTable('t2')

    expect(store()).toMatchObject({ tableId: 't2', customerName: '' })
  })
})
