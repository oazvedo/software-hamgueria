import { beforeEach, describe, expect, it, vi } from 'vitest'
import { act, renderHook, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { useOrderStore } from '@/entities/order'
import { useSubmitOrder } from '@/features/submit-order/model/useSubmitOrder'
import { mockDishes } from '@/shared/mocks/data'
import { QueryWrapper, server } from '@/shared/test'
import { toast } from 'sonner'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

beforeEach(() => {
  vi.clearAllMocks()
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
    server.use(
      http.post('/api/order', () =>
        HttpResponse.json(
          {
            type: 'https://hamgueria.dev/erros/validacao',
            title: 'Alguns dados não passaram na checagem.',
            status: 400,
            errors: { customerName: ['Informe o nome da comanda.'] },
          },
          { status: 400 },
        ),
      ),
    )

    const mutation = await submit()

    expect(mutation.isError).toBe(true)
    expect(useOrderStore.getState().items).toHaveLength(1)
    // The checkout shows field errors next to the field; no generic toast on top of it.
    expect(toast.error).not.toHaveBeenCalled()
  })

  it('003/CA-12: prato inativo mostra a mensagem do sistema no aviso, e não o texto genérico', async () => {
    mockDishes[3].isActive = false
    try {
      const mutation = await submit()

      expect(mutation.isError).toBe(true)
      expect(toast.error).toHaveBeenCalledWith('O prato Burger Monolito não está disponível.')
      expect(useOrderStore.getState().items).toHaveLength(1)
    } finally {
      mockDishes[3].isActive = true
    }
  })

  it('003/CA-12: falha inesperada mostra a mensagem neutra do sistema', async () => {
    server.use(
      http.post('/api/order', () =>
        HttpResponse.json(
          { type: 'https://hamgueria.dev/erros/falha', title: 'Algo deu errado do nosso lado. Tente de novo.', status: 500 },
          { status: 500 },
        ),
      ),
    )

    await submit()

    expect(toast.error).toHaveBeenCalledWith('Algo deu errado do nosso lado. Tente de novo.')
  })

  it('003/CA-12: sem resposta do servidor, o aviso usa o texto da tela', async () => {
    server.use(http.post('/api/order', () => HttpResponse.error()))

    await submit()

    expect(toast.error).toHaveBeenCalledWith('Erro ao enviar o pedido.')
  })
})
