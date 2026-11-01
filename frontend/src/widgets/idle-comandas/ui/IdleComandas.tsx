import { AlarmClock, Lock } from 'lucide-react'
import { useIdleComandas } from '@/entities/order'
import { useCloseComanda } from '@/features/close-comanda'
import { DataText } from '@/shared/ui/atoms'
import { Button } from '@/shared/ui/shadcn'

const IDLE_HOURS = 6

function since(iso: string): string {
  const hours = Math.floor((Date.now() - Date.parse(iso)) / 3_600_000)
  return hours < 48 ? `há ${hours} h` : `há ${Math.floor(hours / 24)} dias`
}

/**
 * Open comandas with no new order for 6 hours (spec 025): a warning, never an automatic close. Closing is a human
 * decision because it cannot be undone (a new order with the same name opens a new comanda).
 */
export function IdleComandas() {
  const { data: idle } = useIdleComandas(IDLE_HOURS)
  const close = useCloseComanda()
  if (!idle || idle.length === 0) return null

  return (
    <section aria-labelledby="comandas-paradas" className="mb-6 rounded-xl border border-destructive/30 bg-destructive/5 p-4">
      <h2 id="comandas-paradas" className="flex items-center gap-2 text-sm font-semibold text-foreground">
        <AlarmClock aria-hidden="true" className="size-4 text-destructive" />
        <DataText>{idle.length}</DataText>
        {idle.length === 1 ? 'comanda aberta sem pedido' : 'comandas abertas sem pedido'} há mais de {IDLE_HOURS} h
      </h2>
      <p className="mt-1 text-xs text-muted-foreground">Confira com o salão antes de fechar: fechar não dá para desfazer.</p>
      <ul className="mt-3 flex flex-col gap-2">
        {idle.map((c) => (
          <li key={`${c.tableId}-${c.customerName}`} className="flex flex-wrap items-center justify-between gap-2 text-sm">
            <span className="text-foreground">
              {c.tableName} · <span className="font-medium">{c.customerName}</span>{' '}
              <span className="text-xs text-muted-foreground">
                último pedido {since(c.lastOrderAt)}, <DataText>{c.orders}</DataText> {c.orders === 1 ? 'pedido' : 'pedidos'}
              </span>
            </span>
            <Button
              size="sm"
              variant="outline"
              disabled={close.isPending}
              onClick={() => close.mutate({ tableId: c.tableId, customerName: c.customerName })}
            >
              <Lock />
              Fechar comanda
            </Button>
          </li>
        ))}
      </ul>
    </section>
  )
}
