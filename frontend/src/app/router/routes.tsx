import { Suspense, lazy } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { Spinner } from '@/shared/ui/shadcn'

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

export function AppRoutes() {
  return (
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
  )
}
