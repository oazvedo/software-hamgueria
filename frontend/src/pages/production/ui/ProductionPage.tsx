import { useState } from 'react'
import { LoginForm, useAuthStore } from '@/features/auth'
import { AuditLog } from '@/widgets/audit-log'
import { KitchenQueue } from '@/widgets/kitchen-queue'
import { KitchenTables } from '@/widgets/kitchen-tables'
import { KitchenTopBar } from '@/widgets/kitchen-top-bar'
import { useAllOrders } from '@/entities/order'
import { Spinner } from '@/shared/ui/shadcn'
import { cn } from '@/shared/lib/cn'

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


/** The kitchen: queue, per-table view and history (the parts are widgets since spec 021). */
function KitchenBoard() {
  const logout = useAuthStore((s) => s.logout)
  const { data: orders, isLoading, isError } = useAllOrders(true)
  const [view, setView] = useState<ViewMode>('fila')

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

        {orders && view === 'fila' && <KitchenQueue orders={orders} />}
        {orders && view === 'mesas' && <KitchenTables orders={orders} />}
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
