import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, LayoutGrid, Lock, Plus, Receipt } from 'lucide-react'
import { Header } from '@/widgets/header'
import { OrderCard, isComandaClosed, isTerminal, orderTotal, useComanda } from '@/entities/order'
import { useCloseComanda } from '@/features/close-comanda'
import { Button, Spinner } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'
import { cn } from '@/shared/lib/cn'

export function ComandaPage() {
  const params = useParams<{ tableId: string; customerName: string }>()
  const customerName = params.customerName ? decodeURIComponent(params.customerName) : undefined
  const { data: orders, isLoading, isFetching } = useComanda(params.tableId, customerName)
  const close = useCloseComanda()

  const sorted = [...(orders ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )
  const grandTotal = sorted.reduce((sum, o) => sum + orderTotal(o), 0)
  const inProduction = sorted.filter((o) => !isTerminal(o.status)).length
  const tableName = sorted[0]?.tableName
  const closed = isComandaClosed(sorted)

  return (
    <div className="flex min-h-full flex-col">
      <Header />
      <main className="mx-auto w-full max-w-2xl flex-1 px-6 py-10">
        <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="inline-flex items-center gap-1.5 font-mono text-xs text-muted-foreground">
              <Receipt className="size-3.5" />
              comanda
            </p>
            <div className="mt-1 flex items-center gap-2">
              <h1 className="text-2xl font-semibold tracking-tight text-foreground">
                {customerName}
              </h1>
              {closed && (
                <span className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary px-2.5 py-0.5 text-xs font-medium text-muted-foreground">
                  <Lock className="size-3" />
                  Fechada
                </span>
              )}
            </div>
            {tableName && <p className="mt-1 text-sm text-muted-foreground">{tableName}</p>}
          </div>
          <div className="flex flex-col items-end gap-2">
            {inProduction > 0 && (
              <span className="inline-flex items-center gap-1.5 font-mono text-xs text-muted-foreground">
                <span className={cn('size-1.5 rounded-full bg-brand', isFetching && 'animate-pulse')} />
                {inProduction} em produção
              </span>
            )}
            <div className="flex items-center gap-2">
              {params.tableId && (
                <Button asChild size="sm" variant="ghost">
                  <Link to={`/mesa/${params.tableId}`}>
                    <LayoutGrid />
                    Ver a mesa toda
                  </Link>
                </Button>
              )}
              {!closed && sorted.length > 0 && params.tableId && customerName && (
                <Button
                  size="sm"
                  variant="outline"
                  disabled={close.isPending}
                  onClick={() => close.mutate({ tableId: params.tableId!, customerName })}
                >
                  <Lock />
                  Fechar comanda
                </Button>
              )}
            </div>
          </div>
        </div>

        {isLoading && (
          <div className="flex justify-center p-16">
            <Spinner className="size-7" />
          </div>
        )}

        {!isLoading && sorted.length === 0 && (
          <div className="rounded-xl border border-dashed border-border p-10 text-center">
            <p className="text-sm text-muted-foreground">Nenhum pedido nesta comanda ainda.</p>
            <Button asChild className="mt-4">
              <Link to="/menu">
                <Plus />
                Fazer o primeiro pedido
              </Link>
            </Button>
          </div>
        )}

        {sorted.length > 0 && (
          <>
            <div className="mb-4 flex items-center justify-between rounded-lg border border-border bg-secondary/40 px-4 py-3">
              <span className="text-sm text-muted-foreground">
                {sorted.length} {sorted.length === 1 ? 'pedido' : 'pedidos'} · total da comanda
              </span>
              <span className="font-mono text-lg font-semibold text-foreground">
                {formatCurrency(grandTotal)}
              </span>
            </div>

            <ul className="flex flex-col gap-4">
              {sorted.map((order) => (
                <li key={order.id}>
                  <OrderCard order={order} />
                </li>
              ))}
            </ul>

            <div className="mt-6 flex justify-center">
              {closed ? (
                <p className="text-sm text-muted-foreground">
                  Comanda fechada. Para pedir de novo, abra uma nova pelo cardápio.
                </p>
              ) : (
                <Button asChild variant="outline">
                  <Link to="/menu">
                    <ArrowLeft />
                    Adicionar mais itens à comanda
                  </Link>
                </Button>
              )}
            </div>
          </>
        )}
      </main>
    </div>
  )
}
