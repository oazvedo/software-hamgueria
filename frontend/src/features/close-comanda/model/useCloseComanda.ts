import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { closeComanda } from '@/entities/order'
import { problemMessage } from '@/shared/api/problem'
import { queryKeys } from '@/shared/api/queryKeys'

export function useCloseComanda() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ tableId, customerName }: { tableId: string; customerName: string }) =>
      closeComanda(tableId, customerName),
    onSuccess: (_data, { tableId, customerName }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.comanda(tableId, customerName) })
      queryClient.invalidateQueries({ queryKey: queryKeys.tableOrders(tableId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.tableComandas(tableId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.allOrders })
      toast.success('Comanda fechada.')
    },
    onError: (error) => toast.error(problemMessage(error, 'Não foi possível fechar a comanda.')),
  })
}
