import { useState } from 'react'
import { Check, ChefHat, Lock, LogOut, X } from 'lucide-react'
import { LoginForm, useAuthStore } from '@/features/auth'
import { useManageOrder } from '@/features/manage-order'
import { useCloseComanda } from '@/features/close-comanda'
import { AuditLog } from '@/widgets/audit-log'
import { ItemNote, StatusBadge, isComandaClosed, isTerminal, orderTotal, useAllOrders } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'
import { DataText } from '@/shared/ui/atoms'
import { ThemeToggle } from '@/shared/ui/molecules'
import { Button, Spinner } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'
import { cn } from '@/shared/lib/cn'

function KitchenTopBar({ onLogout }: { onLogout?: () => void }) {
  return (
    <header className="sticky top-0 z-10 border-b border-border bg-background/80 backdrop-blur">
      <div className="mx-auto flex w-full max-w-[1400px] items-center justify-between px-6 py-3">
        <div className="flex items-center gap-2 font-semibold text-foreground">
          <ChefHat className="size-5 text-brand" />
          Produção
          {/* EXCEÇÃO ao ruido-scan/code-crumb: rótulo de seção em forma de comentário, identidade dev do produto (spec 006, RN-02) */}
          <span className="font-mono text-xs font-normal text-muted-foreground">// cozinha</span>
        </div>
        <div className="flex items-center gap-1">
          <ThemeToggle />
          {onLogout && (
            <Button size="sm" variant="ghost" onClick={onLogout}>
              <LogOut />
              Sair
            </Button>
          )}
        </div>
      </div>
    </header>
  )
}

function waitingLabel(iso: string): string {
  const mins = Math.floor((Date.now() - new Date(iso).getTime()) / 60000)
  if (mins < 1) return 'agora há pouco'
  if (mins === 1) return 'há 1 min'
  return `há ${mins} min`
}

/** Finalizar / Cancelar buttons, reused across queue + table views. */
function OrderActions({ order, className }: { order: OrderResponse; className?: string }) {
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

// ---- View toggle ----

type ViewMode = 'fila' | 'mesas' | 'historico'

function ViewToggle({ value, onChange }: { value: ViewMode; onChange: (v: ViewMode) => void }) {
  const opt = (mode: ViewMode, label: string) => (
    <button
      type="button"
      onClick={() => onChange(mode)}
      className={cn(
        'rounded-md px-3 py-1 text-sm font-medium transition-colors',
        value === mode
          ? 'bg-background text-foreground shadow-xs'
          : 'text-muted-foreground hover:text-foreground',
      )}
    >
      {label}
    </button>
  )
  return (
    <div className="inline-flex rounded-lg border border-border bg-secondary/40 p-1">
      {opt('fila', 'Fila')}
      {opt('mesas', 'Por mesa')}
      {opt('historico', 'Histórico')}
    </div>
  )
}

function KitchenBoard() {
  const logout = useAuthStore((s) => s.logout)
  const { data: orders, isLoading, isError } = useAllOrders(true)
  const [view, setView] = useState<ViewMode>('fila')

  const active = (orders ?? [])
    .filter((o) => !isTerminal(o.status))
    .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
  const finished = (orders ?? [])
    .filter((o) => isTerminal(o.status))
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
  const tables = groupByTable(orders ?? [])

  return (
    <div className="flex min-h-full flex-col">
      <KitchenTopBar onLogout={logout} />
      <main className="mx-auto w-full max-w-[1400px] flex-1 px-6 py-8">
        <div className="mb-6 flex items-center justify-between gap-3">
          <h1 className="text-lg font-semibold text-foreground">Pedidos</h1>
          <ViewToggle value={view} onChange={setView} />
        </div>

        {isLoading && (
          <div className="flex justify-center p-16">
            <Spinner className="size-7" />
          </div>
        )}

        {isError && (
          <p className="rounded-lg border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
            Não foi possível carregar os pedidos. Sua sessão pode ter expirado: saia e entre de novo.
          </p>
        )}

        {orders && view === 'fila' && (
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
        )}

        {orders && view === 'mesas' && (
          <>
            {tables.length === 0 ? (
              <p className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
                Nenhuma comanda aberta.
              </p>
            ) : (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                {tables.map((table) => (
                  <TableCard key={table.tableId} table={table} />
                ))}
              </div>
            )}
          </>
        )}

        {view === 'historico' && <AuditLog />}
      </main>
    </div>
  )
}

export function ProductionPage() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  if (!isAuthenticated) {
    return (
      <div className="flex min-h-full flex-col">
        <KitchenTopBar />
        <LoginForm />
      </div>
    )
  }

  return <KitchenBoard />
}
