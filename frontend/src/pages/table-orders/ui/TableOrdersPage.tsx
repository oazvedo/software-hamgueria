import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, ChevronRight, Lock, Users } from 'lucide-react'
import { Header } from '@/widgets/header'
import { OrderCard, isComandaClosed, isTerminal, orderTotal, useTableOrders } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'
import { Button, Spinner } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'
import { cn } from '@/shared/lib/cn'

interface ComandaGroup {
  customerName: string
  orders: OrderResponse[]
  total: number
  inProduction: number
  closed: boolean
}

function groupByComanda(orders: OrderResponse[]): ComandaGroup[] {
  const map = new Map<string, OrderResponse[]>()
  for (const order of orders) {
    const list = map.get(order.customerName) ?? []
    list.push(order)
    map.set(order.customerName, list)
  }
  return [...map.entries()].map(([customerName, list]) => {
    const sorted = [...list].sort(
      (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    )
    return {
      customerName,
      orders: sorted,
      total: sorted.reduce((sum, o) => sum + orderTotal(o), 0),
      inProduction: sorted.filter((o) => !isTerminal(o.status)).length,
      closed: isComandaClosed(sorted),
    }
  })
}

export function TableOrdersPage() {
  const { tableId } = useParams<{ tableId: string }>()
  const { data: orders, isLoading, isFetching } = useTableOrders(tableId)

  const groups = groupByComanda(orders ?? [])
  const grandTotal = groups.reduce((sum, g) => sum + g.total, 0)
  const tableName = orders?.[0]?.tableName
  const totalInProduction = groups.reduce((sum, g) => sum + g.inProduction, 0)

  return (
    <div className="flex min-h-full flex-col">
      <Header />
      <main className="mx-auto w-full max-w-2xl flex-1 px-6 py-10">
        <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="inline-flex items-center gap-1.5 font-mono text-xs text-muted-foreground">
              <Users className="size-3.5" />
              mesa
            </p>
            <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">
              {tableName ?? 'Mesa'}
            </h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {groups.length} {groups.length === 1 ? 'comanda' : 'comandas'}
            </p>
          </div>
          {totalInProduction > 0 && (
            <span className="inline-flex items-center gap-1.5 font-mono text-xs text-muted-foreground">
              <span className={cn('size-1.5 rounded-full bg-brand', isFetching && 'animate-pulse')} />
              {totalInProduction} em produção
            </span>
          )}
        </div>

        {isLoading && (
          <div className="flex justify-center p-16">
            <Spinner className="size-7" />
          </div>
        )}

        {!isLoading && groups.length === 0 && (
          <div className="rounded-xl border border-dashed border-border p-10 text-center">
            <p className="text-sm text-muted-foreground">Nenhum pedido nesta mesa ainda.</p>
          </div>
        )}

        {groups.length > 0 && (
          <>
            <div className="mb-6 flex items-center justify-between rounded-lg border border-border bg-secondary/40 px-4 py-3">
              <span className="text-sm text-muted-foreground">total da mesa</span>
              <span className="font-mono text-lg font-semibold text-foreground">
                {formatCurrency(grandTotal)}
              </span>
            </div>

            <div className="flex flex-col gap-8">
              {groups.map((group) => (
                <section key={group.customerName}>
                  <div className="mb-3 flex items-center justify-between gap-3">
                    <div className="flex items-center gap-2">
                      <Link
                        to={`/comanda/${tableId}/${encodeURIComponent(group.customerName)}`}
                        className="group inline-flex items-center gap-1.5"
                      >
                        <h2 className="font-semibold text-foreground group-hover:underline">
                          {group.customerName}
                        </h2>
                        <ChevronRight className="size-4 text-muted-foreground" />
                        <span className="font-mono text-xs text-muted-foreground">
                          {group.orders.length} {group.orders.length === 1 ? 'pedido' : 'pedidos'}
                        </span>
                      </Link>
                      {group.closed && (
                        <span className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary px-2 py-0.5 text-xs text-muted-foreground">
                          <Lock className="size-3" />
                          Fechada
                        </span>
                      )}
                    </div>
                    <span className="font-mono text-sm font-semibold text-foreground">
                      {formatCurrency(group.total)}
                    </span>
                  </div>
                  <ul className="flex flex-col gap-3">
                    {group.orders.map((order) => (
                      <li key={order.id}>
                        <OrderCard order={order} />
                      </li>
                    ))}
                  </ul>
                </section>
              ))}
            </div>

            <div className="mt-8 flex justify-center">
              <Button asChild variant="outline">
                <Link to="/menu">
                  <ArrowLeft />
                  Voltar ao cardápio
                </Link>
              </Button>
            </div>
          </>
        )}
      </main>
    </div>
  )
}
