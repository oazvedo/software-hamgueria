import { beforeEach, describe, expect, it } from 'vitest'
import { act, renderHook, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { useOrderStore } from '@/entities/order'
import { useSubmitOrder } from '@/features/submit-order/model/useSubmitOrder'
import { mockDishes } from '@/shared/mocks/data'
import { QueryWrapper, server } from '@/shared/test'

beforeEach(() => {
  useOrderStore.setState({ tableId: 't1', customerName: 'Ana', items: [] })
  useOrderStore.getState().addDish(mockDishes[3])
})

const submit = async () => {
  const { result } = renderHook(() => useSubmitOrder(), { wrapper: QueryWrapper })
  act(() => result.current.mutate())
  await waitFor(() => expect(result.current.isPending).toBe(false))
  return result.current
}

describe('envio do pedido', () => {
  it('001/CA-03: pedido enviado é criado como recebido e o carrinho esvazia, mantendo mesa e nome', async () => {
    const mutation = await submit()

    expect(mutation.isSuccess).toBe(true)
    expect(mutation.data).toMatchObject({ status: 'Created', customerName: 'Ana', tableId: 't1' })
    expect(useOrderStore.getState()).toMatchObject({ items: [], tableId: 't1', customerName: 'Ana' })
  })

  it('001/CA-04: se o servidor recusa o pedido, o carrinho é mantido', async () => {
    server.use(http.post('/api/order', () => new HttpResponse('Customer name (comanda) is required.', { status: 400 })))

    const mutation = await submit()

    expect(mutation.isError).toBe(true)
    expect(useOrderStore.getState().items).toHaveLength(1)
  })
})
