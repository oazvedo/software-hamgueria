import { Link, Navigate, useParams } from 'react-router-dom'
import { ArrowLeft, ChevronRight, Lock, Users } from 'lucide-react'
import { Header } from '@/widgets/header'
import { useOrderStore, useTableOrders } from '@/entities/order'
import { summarizeComandas, type ComandaSummary } from '@/pages/table-comandas/model/summarizeComandas'
import { DataText } from '@/shared/ui/atoms'
import { Button, Spinner } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'
import { cn } from '@/shared/lib/cn'

function ComandaRow({ tableId, comanda, mine }: { tableId: string; comanda: ComandaSummary; mine: boolean }) {
  return (
    <li>
      <Link
        to={`/comanda/${tableId}/${encodeURIComponent(comanda.customerName)}`}
        className={cn(
          'flex items-center gap-3 rounded-xl border bg-card px-4 py-3 transition-colors hover:bg-secondary/60',
          mine ? 'border-brand/50' : 'border-border',
          comanda.closed && 'opacity-70',
        )}
      >
        <div className="min-w-0 flex-1">
          <p className="flex flex-wrap items-center gap-2">
            <span className="truncate font-medium text-foreground">{comanda.customerName}</span>
            {mine && <span className="text-xs text-brand">sua comanda</span>}
            {comanda.closed ? (
              <span className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary px-2 py-0.5 text-xs text-muted-foreground">
                <Lock aria-hidden="true" className="size-3" />
                Fechada
              </span>
            ) : (
              <span className="rounded-full border border-brand/30 bg-brand/10 px-2 py-0.5 text-xs text-brand">
                Aberta
              </span>
            )}
          </p>
          <p className="mt-0.5 text-xs text-muted-foreground">
            <DataText>{comanda.orders}</DataText> {comanda.orders === 1 ? 'pedido' : 'pedidos'}
            {comanda.inProduction > 0 && (
              <>
                {' · '}
                <DataText>{comanda.inProduction}</DataText> em produção
              </>
            )}
          </p>
        </div>
        <DataText className="text-sm font-semibold text-foreground">{formatCurrency(comanda.total)}</DataText>
        <ChevronRight aria-hidden="true" className="size-4 shrink-0 text-muted-foreground" />
      </Link>
    </li>
  )
}

/** Every comanda of a table, open first, each leading to its detail (spec 007). */
export function TableComandasPage() {
  const { tableId = '' } = useParams<{ tableId: string }>()
  const { data: orders, isLoading } = useTableOrders(tableId)
  const myTable = useOrderStore((s) => s.tableId)
  const myName = useOrderStore((s) => s.customerName.trim())

  const comandas = summarizeComandas(orders ?? [])
  const open = comandas.filter((c) => !c.closed).length
  const tableTotal = comandas.reduce((sum, c) => sum + c.total, 0)
  const tableName = orders?.[0]?.tableName

  return (
    <div className="flex min-h-full flex-col">
      <Header />
      <main className="mx-auto w-full max-w-2xl flex-1 px-6 py-10">
        <div className="mb-6">
          <p className="inline-flex items-center gap-1.5 text-xs text-muted-foreground">
            <Users aria-hidden="true" className="size-3.5" />
            Comandas da mesa
          </p>
          <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">{tableName ?? 'Mesa'}</h1>
        </div>

        {isLoading && (
          <div className="flex justify-center p-16">
            <Spinner className="size-7" />
          </div>
        )}

        {!isLoading && comandas.length === 0 && (
          <div className="flex flex-col items-center gap-4 rounded-xl border border-dashed border-border p-10 text-center">
            <p className="text-sm text-muted-foreground">Nenhuma comanda nesta mesa ainda.</p>
            <Button asChild variant="outline">
              <Link to="/menu">
                <ArrowLeft />
                Ir para o cardápio
              </Link>
            </Button>
          </div>
        )}

        {comandas.length > 0 && (
          <>
            <div className="mb-4 flex items-center justify-between rounded-lg border border-border bg-secondary/40 px-4 py-3">
              <span className="text-sm text-muted-foreground">
                <DataText>{comandas.length}</DataText> {comandas.length === 1 ? 'comanda' : 'comandas'},{' '}
                <DataText>{open}</DataText> {open === 1 ? 'aberta' : 'abertas'}
              </span>
              <DataText className="text-lg font-semibold text-foreground">{formatCurrency(tableTotal)}</DataText>
            </div>

            <ul className="flex flex-col gap-2" aria-label="Comandas da mesa">
              {comandas.map((comanda) => (
                <ComandaRow
                  key={comanda.customerName}
                  tableId={tableId}
                  comanda={comanda}
                  mine={myTable === tableId && myName === comanda.customerName}
                />
              ))}
            </ul>

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

/** The old "/mesa/:tableId" address now lists the comandas (spec 007, CA-06). */
export function LegacyTableRedirect() {
  const { tableId = '' } = useParams<{ tableId: string }>()
  return <Navigate to={`/comandas/${tableId}`} replace />
}
