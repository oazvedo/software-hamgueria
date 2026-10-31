import { Check, X } from 'lucide-react'
import type { OrderResponse } from '@/entities/order'
import { useManageOrder } from '@/features/manage-order/model/useManageOrder'
import { Button } from '@/shared/ui/shadcn'
import { cn } from '@/shared/lib/cn'

/** Finalizar / Cancelar for the kitchen, used by the queue and the per-table view. */
export function OrderActions({ order, className }: { order: OrderResponse; className?: string }) {
  const { finish, cancel } = useManageOrder()
  const busy =
    (finish.isPending && finish.variables === order.id) ||
    (cancel.isPending && cancel.variables === order.id)

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Button size="sm" className="flex-1" disabled={busy} onClick={() => finish.mutate(order.id)}>
        <Check />
        Finalizar
      </Button>
      <Button size="sm" variant="outline" disabled={busy} onClick={() => cancel.mutate(order.id)}>
        <X />
        Cancelar
      </Button>
    </div>
  )
}
