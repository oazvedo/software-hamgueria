import { beforeEach, describe, expect, it, vi } from 'vitest'
import { act, renderHook, waitFor } from '@testing-library/react'
import { toast } from 'sonner'
import { useManageOrder } from '@/features/manage-order'
import { clearToken, setToken } from '@/shared/auth/authToken'
import { QueryWrapper } from '@/shared/test'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

beforeEach(() => {
  vi.clearAllMocks()
  clearToken()
})

function render() {
  return renderHook(() => useManageOrder(), { wrapper: QueryWrapper }).result
}

describe('ações da cozinha', () => {
  it('003/CA-14: finalizar pedido inexistente mostra a mensagem do sistema', async () => {
    setToken('mock-kitchen-jwt')
    const result = render()

    act(() => result.current.finish.mutate('nao-existe'))
    await waitFor(() => expect(result.current.finish.isError).toBe(true))

    expect(toast.error).toHaveBeenCalledWith('Pedido não encontrado.')
  })

  it('003/CA-14: cancelar pedido inexistente mostra a mensagem do sistema', async () => {
    const result = render()

    act(() => result.current.cancel.mutate('nao-existe'))
    await waitFor(() => expect(result.current.cancel.isError).toBe(true))

    expect(toast.error).toHaveBeenCalledWith('Pedido não encontrado.')
  })

  it('003/CA-14: finalizar sem login mostra o motivo do sistema', async () => {
    const result = render()

    act(() => result.current.finish.mutate('qualquer'))
    await waitFor(() => expect(result.current.finish.isError).toBe(true))

    expect(toast.error).toHaveBeenCalledWith('Faça login na cozinha para continuar.')
  })
})
