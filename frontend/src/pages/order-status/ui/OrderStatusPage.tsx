import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, Rocket, Users, X } from 'lucide-react'
import { Header } from '@/widgets/header'
import { OwnOrderActions } from '@/features/change-own-order'
import { ItemNote, OrderPipeline, StatusBadge, isTerminal, useOrder } from '@/entities/order'
import { Alert, AlertDescription, AlertTitle, Button, Spinner } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'
import { cn } from '@/shared/lib/cn'

export function OrderStatusPage() {
  const { id } = useParams<{ id: string }>()
  const { data: order, isLoading, isError, isFetching } = useOrder(id)

  return (
    <div className="flex min-h-full flex-col">
      <Header />
      <main className="mx-auto w-full max-w-2xl flex-1 px-6 py-10">
        {isLoading && (
          <div className="flex justify-center p-16">
            <Spinner className="size-7" />
          </div>
        )}

        {isError && (
          <Alert variant="destructive">
            <X />
            <AlertTitle>Pedido não encontrado</AlertTitle>
            <AlertDescription>
              O id informado não existe ou o pedido expirou.
              <Link to="/menu" className="mt-2 inline-block underline underline-offset-4">
                Voltar ao cardápio
              </Link>
            </AlertDescription>
          </Alert>
        )}

        {order && (
          <div className="flex flex-col gap-6">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="font-mono text-xs text-muted-foreground">
                  order/<span className="text-foreground">{order.id.slice(0, 8)}</span>
                </p>
                <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">
                  Acompanhando seu pedido
                </h1>
                <p className="mt-1 text-sm text-muted-foreground">
                  comanda <span className="text-foreground">{order.customerName}</span> ·{' '}
                  {order.tableName}
                </p>
              </div>
              <StatusBadge status={order.status} />
            </div>

            <section className="rounded-xl border border-border bg-card p-6">
              <div className="mb-4 flex items-center justify-between">
                {/* EXCEÇÃO ao ruido-scan/code-crumb: rótulo de seção em forma de comentário, identidade dev do produto (spec 006, RN-02) */}
                <h2 className="font-mono text-sm text-muted-foreground">// pipeline</h2>
                {!isTerminal(order.status) && (
                  <span className="inline-flex items-center gap-1.5 font-mono text-xs text-muted-foreground">
                    <span className={cn('size-1.5 rounded-full bg-brand', isFetching && 'animate-pulse')} />
                    monitorando ao vivo
                  </span>
                )}
              </div>
              <OrderPipeline status={order.status} />
            </section>

            <section className="rounded-xl border border-border bg-card p-6">
              {/* EXCEÇÃO ao ruido-scan/code-crumb: rótulo de seção em forma de comentário, identidade dev do produto (spec 006, RN-02) */}
              <h2 className="mb-3 font-mono text-sm text-muted-foreground">// itens</h2>
              <ul className="flex flex-col divide-y divide-border">
                {order.items.map((item) => (
                  <li key={item.dishId} className="flex items-start justify-between gap-3 py-2 text-sm">
                    <span className="text-foreground">
                      <span className="font-mono text-muted-foreground">{item.quantity}×</span>{' '}
                      {item.dishName}
                      {item.notes && <ItemNote notes={item.notes} className="text-muted-foreground" />}
                    </span>
                    <span className="font-mono text-muted-foreground">
                      {formatCurrency(item.price * item.quantity)}
                    </span>
                  </li>
                ))}
              </ul>
              <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
                <span className="text-sm text-muted-foreground">total</span>
                <span className="font-mono font-semibold text-foreground">
                  {formatCurrency(order.items.reduce((s, it) => s + it.price * it.quantity, 0))}
                </span>
              </div>
            </section>

            {order.status === 'Done' && (
              <div className="flex items-center gap-2 rounded-lg border border-brand/30 bg-brand/5 px-4 py-3 text-sm text-foreground">
                <Rocket aria-hidden="true" className="size-4 text-brand" />
                Pedido servido. Deploy em produção concluído, bom apetite!
              </div>
            )}

            <OwnOrderActions order={order} />

            <div className="flex flex-wrap justify-center gap-3">
              <Button asChild variant="secondary">
                <Link to={`/comanda/${order.tableId}/${encodeURIComponent(order.customerName)}`}>
                  <Users />
                  Ver comanda completa
                </Link>
              </Button>
              <Button asChild variant="outline">
                <Link to="/menu">
                  <ArrowLeft />
                  Adicionar mais itens
                </Link>
              </Button>
            </div>
          </div>
        )}
      </main>
    </div>
  )
}
