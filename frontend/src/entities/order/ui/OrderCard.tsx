import { Link } from 'react-router-dom'
import { OrderPipeline } from '@/entities/order/ui/OrderPipeline'
import { StatusBadge } from '@/entities/order/ui/StatusBadge'
import { orderTotal } from '@/entities/order/model/orderTotal'
import type { OrderResponse } from '@/entities/order/api/orderApi'
import { formatCurrency } from '@/shared/lib/formatCurrency'

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

/** A single order summary card: id/time, status, compact pipeline, items, total. */
export function OrderCard({ order }: { order: OrderResponse }) {
  return (
    <div className="rounded-xl border border-border bg-card p-5">
      <div className="flex items-center justify-between gap-3">
        <div className="font-mono text-xs text-muted-foreground">
          order/<span className="text-foreground">{order.id.slice(0, 8)}</span>
          <span className="mx-2">·</span>
          {formatTime(order.createdAt)}
        </div>
        <StatusBadge status={order.status} />
      </div>

      <div className="my-4">
        <OrderPipeline status={order.status} variant="compact" />
      </div>

      <ul className="flex flex-col gap-1 text-sm">
        {order.items.map((item) => (
          <li key={item.dishId} className="flex items-start justify-between gap-3">
            <span className="text-foreground">
              <span className="font-mono text-muted-foreground">{item.quantity}×</span> {item.dishName}
              {item.notes && (
                <span className="mt-0.5 block text-xs text-muted-foreground">↳ {item.notes}</span>
              )}
            </span>
            <span className="font-mono text-muted-foreground">
              {formatCurrency(item.price * item.quantity)}
            </span>
          </li>
        ))}
      </ul>

      <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
        <Link
          to={`/order/${order.id}`}
          className="font-mono text-xs text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
        >
          ver detalhe →
        </Link>
        <span className="font-mono font-semibold text-foreground">{formatCurrency(orderTotal(order))}</span>
      </div>
    </div>
  )
}
