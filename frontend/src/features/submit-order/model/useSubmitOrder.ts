import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { buildOrderPayload, createOrder, useOrderStore } from '@/entities/order'
import { fieldErrors, problemMessage } from '@/shared/api/problem'

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
    onError: (error) => {
      // Field errors are shown next to each field by the checkout (spec 003, CA-11). The cart stays as it was.
      if (Object.keys(fieldErrors(error)).length > 0) return
      toast.error(problemMessage(error, 'Erro ao enviar o pedido.'))
    },
  })
}
