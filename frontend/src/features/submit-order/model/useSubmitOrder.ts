import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { buildOrderPayload, createOrder, useOrderStore } from '@/entities/order'

export function useSubmitOrder() {
  const clear = useOrderStore((s) => s.clear)

  return useMutation({
    mutationFn: () => {
      const { tableId, customerName, items } = useOrderStore.getState()
      return createOrder(buildOrderPayload({ tableId, customerName, items }))
    },
    onSuccess: () => {
      clear()
      toast.success('Pedido enviado com sucesso.')
    },
    onError: () => {
      toast.error('Erro ao enviar o pedido.')
    },
  })
}
