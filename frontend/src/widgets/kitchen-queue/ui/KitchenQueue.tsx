import { ItemNote, StatusBadge, isTerminal, orderTotal, waitingLabel } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'
import { OrderActions } from '@/features/manage-order'
import { DataText } from '@/shared/ui/atoms'
import { formatCurrency } from '@/shared/lib/formatCurrency'

function ActiveCard({ order }: { order: OrderResponse }) {
  return (
    <div className="flex flex-col rounded-xl border border-border bg-card p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="font-semibold text-foreground">{order.tableName}</p>
          <p className="text-sm text-muted-foreground">
            comanda <span className="text-foreground">{order.customerName}</span>
          </p>
        </div>
        <DataText className="text-xs text-muted-foreground">{waitingLabel(order.createdAt)}</DataText>
      </div>

      <ul className="my-4 flex flex-col gap-1.5 text-sm">
        {order.items.map((item) => (
          <li key={item.dishId}>
            <span className="text-foreground">
              <DataText className="text-muted-foreground">{item.quantity}×</DataText> {item.dishName}
            </span>
            {item.notes && <ItemNote notes={item.notes} className="font-medium text-brand" />}
          </li>
        ))}
      </ul>

      <OrderActions order={order} className="mt-auto border-t border-border pt-3" />
    </div>
  )
}


function FinishedRow({ order }: { order: OrderResponse }) {
  return (
    <li className="flex items-center justify-between gap-3 rounded-lg border border-border bg-card/60 px-4 py-2.5 text-sm">
      <div className="min-w-0">
        <span className="text-foreground">{order.tableName}</span>
        <span className="mx-2 text-muted-foreground">·</span>
        <span className="text-muted-foreground">{order.customerName}</span>
      </div>
      <div className="flex items-center gap-3">
        <DataText className="text-xs text-muted-foreground">{formatCurrency(orderTotal(order))}</DataText>
        <StatusBadge status={order.status} />
      </div>
    </li>
  )
}

// ---- "Por mesa" grouped view ----


/** Orders in progress, oldest first, and the ones finished in the last 24 h (spec 020), newest first. */
export function KitchenQueue({ orders }: { orders: OrderResponse[] }) {
  const active = orders
    .filter((o) => !isTerminal(o.status))
    .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
  const finished = orders
    .filter((o) => isTerminal(o.status))
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())

  return (
    <div className="grid grid-cols-1 gap-8 lg:grid-cols-[1fr_360px]">
      <section>
        <h2 className="mb-4 flex items-center gap-2 text-base font-semibold text-foreground">
          Em produção
          <DataText className="text-sm font-normal text-muted-foreground">({active.length})</DataText>
        </h2>
        {active.length === 0 ? (
          <p className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
            Nenhum pedido na fila. A cozinha está em dia.
          </p>
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {active.map((order) => (
              <ActiveCard key={order.id} order={order} />
            ))}
          </div>
        )}
      </section>

      <aside>
        <h2 className="mb-4 flex items-center gap-2 text-base font-semibold text-foreground">
          Finalizados
          <span className="text-xs font-normal text-muted-foreground">últimas 24 h</span>
          <DataText className="text-sm font-normal text-muted-foreground">({finished.length})</DataText>
        </h2>
        {finished.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nada finalizado ainda.</p>
        ) : (
          <ul className="flex flex-col gap-2">
            {finished.map((order) => (
              <FinishedRow key={order.id} order={order} />
            ))}
          </ul>
        )}
      </aside>
    </div>
  )
}
