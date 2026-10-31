import { beforeEach, describe, expect, it, vi } from 'vitest'
import { act, renderHook, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { toast } from 'sonner'
import { useCloseComanda } from '@/features/close-comanda'
import { QueryWrapper, server } from '@/shared/test'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

beforeEach(() => vi.clearAllMocks())

async function close(customerName: string) {
  const { result } = renderHook(() => useCloseComanda(), { wrapper: QueryWrapper })
  act(() => result.current.mutate({ tableId: 't1', customerName }))
  await waitFor(() => expect(result.current.isPending).toBe(false))
  return result.current
}

describe('fechar comanda', () => {
  it('001/CA-08: comanda com nome válido é fechada', async () => {
    expect((await close('Ana')).isSuccess).toBe(true)
    expect(toast.success).toHaveBeenCalledWith('Comanda fechada.')
  })

  it('003/CA-12: recusa do servidor aparece no aviso com a mensagem do sistema', async () => {
    await close('a'.repeat(61))

    expect(toast.error).toHaveBeenCalledWith('O nome da comanda pode ter no máximo 60 caracteres.')
  })

  it('003/CA-12: sem resposta do servidor, o aviso usa o texto da tela', async () => {
    server.use(http.post('/api/order/comanda/close', () => HttpResponse.error()))

    await close('Ana')

    expect(toast.error).toHaveBeenCalledWith('Não foi possível fechar a comanda.')
  })
})
