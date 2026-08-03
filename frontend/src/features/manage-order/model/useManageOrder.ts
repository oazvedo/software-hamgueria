import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { cancelOrder, updateOrderStatus } from '@/entities/order'
import { queryKeys } from '@/shared/api/queryKeys'

export function useManageOrder() {
  const queryClient = useQueryClient()
  const invalidate = () => queryClient.invalidateQueries({ queryKey: queryKeys.allOrders })

  const finish = useMutation({
    mutationFn: (id: string) => updateOrderStatus(id, 'Done'),
    onSuccess: () => {
      invalidate()
      toast.success('Pedido finalizado.')
    },
    onError: () => toast.error('Não foi possível finalizar o pedido.'),
  })

  const cancel = useMutation({
    mutationFn: (id: string) => cancelOrder(id),
    onSuccess: () => {
      invalidate()
      toast.success('Pedido cancelado.')
    },
    onError: () => toast.error('Não foi possível cancelar o pedido.'),
  })

  return { finish, cancel }
}
