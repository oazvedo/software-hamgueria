import { Lock } from 'lucide-react'
import { ItemNote, StatusBadge, isComandaClosed, isTerminal, orderTotal, waitingLabel } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'
import { useCloseComanda } from '@/features/close-comanda'
import { OrderActions } from '@/features/manage-order'
import { DataText } from '@/shared/ui/atoms'
import { Button } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'

interface ComandaGroup {
  customerName: string
  orders: OrderResponse[]
  total: number
  active: number
  closed: boolean
}
interface TableGroup {
  tableId: string
  tableName: string
  comandas: ComandaGroup[]
  total: number
}

function groupByTable(orders: OrderResponse[]): TableGroup[] {
  const tables = new Map<string, OrderResponse[]>()
  for (const o of orders) {
    const list = tables.get(o.tableId) ?? []
    list.push(o)
    tables.set(o.tableId, list)
  }

  return [...tables.values()]
    .map((tableOrders) => {
      const comandaMap = new Map<string, OrderResponse[]>()
      for (const o of tableOrders) {
        const list = comandaMap.get(o.customerName) ?? []
        list.push(o)
        comandaMap.set(o.customerName, list)
      }
      const comandas: ComandaGroup[] = [...comandaMap.entries()].map(([customerName, list]) => ({
        customerName,
        orders: list.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()),
        total: list.reduce((s, o) => s + orderTotal(o), 0),
        active: list.filter((o) => !isTerminal(o.status)).length,
        closed: isComandaClosed(list),
      }))
      return {
        tableId: tableOrders[0].tableId,
        tableName: tableOrders[0].tableName,
        comandas,
        total: comandas.reduce((s, c) => s + c.total, 0),
      }
    })
    .sort((a, b) => a.tableName.localeCompare(b.tableName))
}

function OrderRow({ order }: { order: OrderResponse }) {
  const active = !isTerminal(order.status)
  return (
    <div className="rounded-lg border border-border bg-background/40 p-3">
      <div className="flex items-center justify-between gap-3">
        <DataText className="text-xs text-muted-foreground">
          order/<span className="text-foreground">{order.id.slice(0, 8)}</span>
          <span className="mx-2">·</span>
          {waitingLabel(order.createdAt)}
        </DataText>
        {/* The badge keeps its one line on narrow screens; the id/time line wraps instead. */}
        <span className="shrink-0">
          <StatusBadge status={order.status} />
        </span>
      </div>
      <ul className="mt-2 flex flex-col gap-1 text-sm">
        {order.items.map((item) => (
          <li key={item.dishId}>
            <span className="text-foreground">
              <DataText className="text-muted-foreground">{item.quantity}×</DataText> {item.dishName}
            </span>
            {item.notes && <ItemNote notes={item.notes} inline className="font-medium text-brand" />}
          </li>
        ))}
      </ul>
      {active && <OrderActions order={order} className="mt-3" />}
    </div>
  )
}

function TableCard({ table }: { table: TableGroup }) {
  const close = useCloseComanda()
  return (
    <section className="rounded-xl border border-border bg-card p-5">
      <div className="mb-3 flex items-center justify-between gap-3 border-b border-border pb-3">
        <h3 className="font-semibold text-foreground">{table.tableName}</h3>
        <span className="text-xs text-muted-foreground">
          <DataText>{table.comandas.length}</DataText> {table.comandas.length === 1 ? 'comanda' : 'comandas'} ·{' '}
          <DataText>{formatCurrency(table.total)}</DataText>
        </span>
      </div>

      <div className="flex flex-col gap-4">
        {table.comandas.map((comanda) => (
          <div key={comanda.customerName}>
            <div className="mb-2 flex items-center justify-between gap-2">
              <p className="flex items-center gap-2 font-medium text-foreground">
                {comanda.customerName}
                {comanda.closed ? (
                  <span className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary px-2 py-0.5 text-xs font-normal text-muted-foreground">
                    <Lock className="size-3" />
                    Fechada
                  </span>
                ) : (
                  comanda.active > 0 && (
                    <span className="text-xs text-brand">
                      <DataText>{comanda.active}</DataText> em produção
                    </span>
                  )
                )}
              </p>
              <div className="flex items-center gap-2">
                <DataText className="text-xs text-muted-foreground">{formatCurrency(comanda.total)}</DataText>
                {!comanda.closed && (
                  <Button
                    size="sm"
                    variant="ghost"
                    disabled={close.isPending}
                    onClick={() =>
                      close.mutate({ tableId: table.tableId, customerName: comanda.customerName })
                    }
                  >
                    <Lock />
                    Fechar
                  </Button>
                )}
              </div>
            </div>
            <div className="flex flex-col gap-2">
              {comanda.orders.map((order) => (
                <OrderRow key={order.id} order={order} />
              ))}
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}

/** The kitchen's per-table view: each table, its comandas and their orders. */
export function KitchenTables({ orders }: { orders: OrderResponse[] }) {
  const tables = groupByTable(orders)
  if (tables.length === 0) {
    return (
      <p className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
        Nenhuma comanda aberta.
      </p>
    )
  }
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
      {tables.map((table) => (
        <TableCard key={table.tableId} table={table} />
      ))}
    </div>
  )
}
