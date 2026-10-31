import { Component, Suspense, lazy, type ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { RotateCw } from 'lucide-react'
import { Button, Spinner } from '@/shared/ui/shadcn'

// One chunk per page (spec 021): the customer never downloads the kitchen, and the first screen loads less.
const HomePage = lazy(() => import('@/pages/home').then((m) => ({ default: m.HomePage })))
const MenuPage = lazy(() => import('@/pages/menu').then((m) => ({ default: m.MenuPage })))
const OrderStatusPage = lazy(() => import('@/pages/order-status').then((m) => ({ default: m.OrderStatusPage })))
const ComandaPage = lazy(() => import('@/pages/comanda').then((m) => ({ default: m.ComandaPage })))
const TableComandasPage = lazy(() => import('@/pages/table-comandas').then((m) => ({ default: m.TableComandasPage })))
const LegacyTableRedirect = lazy(() => import('@/pages/table-comandas').then((m) => ({ default: m.LegacyTableRedirect })))
const ProductionPage = lazy(() => import('@/pages/production').then((m) => ({ default: m.ProductionPage })))

function PageLoading() {
  return (
    <div className="flex min-h-full items-center justify-center p-16" role="status" aria-label="Carregando a página">
      <Spinner className="size-7" />
    </div>
  )
}

/**
 * A page chunk that fails to download (a deploy removed the old file, the network dropped) would unmount the whole app
 * into a blank screen. This catches it and offers a reload, which fetches the current version (spec 021, CA-02).
 */
export class ChunkErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }

  static getDerivedStateFromError() {
    return { failed: true }
  }

  render() {
    if (!this.state.failed) return this.props.children
    return (
      <div role="alert" className="flex min-h-full flex-col items-center justify-center gap-4 p-16 text-center">
        <p className="text-sm text-foreground">Não foi possível carregar esta tela.</p>
        <p className="max-w-sm text-xs text-muted-foreground">
          Pode ser uma versão nova do site ou a conexão. Recarregar busca a versão atual.
        </p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          <RotateCw />
          Recarregar
        </Button>
      </div>
    )
  }
}

export function AppRoutes() {
  return (
    <ChunkErrorBoundary>
      <Suspense fallback={<PageLoading />}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/menu" element={<MenuPage />} />
          <Route path="/order/:id" element={<OrderStatusPage />} />
          <Route path="/comanda/:tableId/:customerName" element={<ComandaPage />} />
          <Route path="/comandas/:tableId" element={<TableComandasPage />} />
          <Route path="/mesa/:tableId" element={<LegacyTableRedirect />} />
          <Route path="/producao" element={<ProductionPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
    </ChunkErrorBoundary>
  )
}
