import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { cancelOrder, updateOrder } from '@/entities/order'
import { problemMessage } from '@/shared/api/problem'
import { queryKeys } from '@/shared/api/queryKeys'

/** The customer's own changes to an order made on this device (spec 015). */
export function useChangeOwnOrder(orderId: string) {
  const queryClient = useQueryClient()
  const refresh = () => queryClient.invalidateQueries({ queryKey: queryKeys.order(orderId) })

  const cancel = useMutation({
    mutationFn: () => cancelOrder(orderId),
    onSuccess: () => {
      refresh()
      toast.success('Pedido cancelado. A cozinha não vai prepará-lo.')
    },
    onError: (error) => toast.error(problemMessage(error, 'Não foi possível cancelar o pedido.')),
  })

  const update = useMutation({
    mutationFn: (items: Array<{ dishId: string; quantity: number; notes?: string }>) => updateOrder(orderId, items),
    onSuccess: () => {
      refresh()
      toast.success('Pedido alterado. A cozinha já recebeu a nova versão.')
    },
    onError: (error) => toast.error(problemMessage(error, 'Não foi possível alterar o pedido.')),
  })

  return { cancel, update }
}
