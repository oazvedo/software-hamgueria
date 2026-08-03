import { Link } from 'react-router-dom'
import { Receipt } from 'lucide-react'
import { Header } from '@/widgets/header'
import { MenuCatalog } from '@/widgets/menu-catalog'
import { OrderJsonPanel } from '@/widgets/order-json'
import { useOrderStore } from '@/entities/order'
import { useTables } from '@/entities/table'

function ComandaChip() {
  const tableId = useOrderStore((s) => s.tableId)
  const customerName = useOrderStore((s) => s.customerName)
  const { data: tables } = useTables()
  if (!tableId || !customerName.trim()) return null

  const tableName = tables?.find((t) => t.id === tableId)?.tableName

  return (
    <Link
      to={`/comanda/${tableId}/${encodeURIComponent(customerName)}`}
      className="inline-flex items-center gap-1.5 rounded-full border border-border bg-secondary/40 px-3 py-1.5 text-xs text-muted-foreground transition-colors hover:border-muted-foreground/40 hover:text-foreground"
    >
      <Receipt className="size-3.5" />
      <span className="font-medium text-foreground">{customerName}</span>
      {tableName && (
        <>
          <span className="text-muted-foreground/60">·</span>
          <span>{tableName}</span>
        </>
      )}
    </Link>
  )
}

export function MenuPage() {
  return (
    <div className="flex min-h-full flex-col">
      <Header />
      <main className="mx-auto grid w-full max-w-[1600px] flex-1 grid-cols-1 gap-8 px-6 py-8 lg:grid-cols-[minmax(0,1fr)_500px]">
        <section className="min-w-0">
          <div className="mb-6 flex items-start justify-between gap-3">
            <div>
              <h1 className="text-2xl font-semibold tracking-tight text-foreground">Cardápio</h1>
              <p className="mt-1 font-mono text-sm text-muted-foreground">// monte seu pedido</p>
            </div>
            <ComandaChip />
          </div>
          <MenuCatalog />
        </section>
        <aside className="lg:sticky lg:top-20 lg:h-[calc(100vh-6rem)]">
          <OrderJsonPanel />
        </aside>
      </main>
    </div>
  )
}
