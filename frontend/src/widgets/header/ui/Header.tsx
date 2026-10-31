import { Link } from 'react-router-dom'
import { useOrderStore } from '@/entities/order'

export function Header() {
  const tableId = useOrderStore((s) => s.tableId)
  const customerName = useOrderStore((s) => s.customerName)
  const hasComanda = Boolean(tableId && customerName.trim())

  const linkClass =
    'rounded-md px-2 py-1 text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground'

  return (
    <header className="sticky top-0 z-10 border-b border-border bg-background/80 backdrop-blur">
      <div className="mx-auto flex w-full max-w-[1600px] items-center justify-between px-6 py-3">
        <Link to="/" className="flex items-center gap-2 font-semibold text-foreground">
          <span className="font-mono text-brand">{'</>'}</span>
          <span>Software Hamgueria</span>
        </Link>
        <nav className="flex items-center gap-1 font-mono text-sm">
          {/* EXCEÇÃO ao ruido-scan/code-crumb: navegação em forma de caminho de terminal, identidade dev do produto (spec 006, RN-02) */}
          <Link to="/menu" className={linkClass}>
            ~/menu
          </Link>
          {hasComanda && (
            <>
              {/* EXCEÇÃO ao ruido-scan/code-crumb: navegação em forma de caminho de terminal, identidade dev do produto (spec 006, RN-02) */}
              <Link
                to={`/comanda/${tableId}/${encodeURIComponent(customerName)}`}
                className={linkClass}
              >
                ~/comanda
              </Link>
              {/* EXCEÇÃO ao ruido-scan/code-crumb: navegação em forma de caminho de terminal, identidade dev do produto (spec 006, RN-02) */}
              <Link to={`/mesa/${tableId}`} className={linkClass}>
                ~/mesa
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  )
}
