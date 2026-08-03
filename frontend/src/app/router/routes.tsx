import { Navigate, Route, Routes } from 'react-router-dom'
import { HomePage } from '@/pages/home'
import { MenuPage } from '@/pages/menu'
import { OrderStatusPage } from '@/pages/order-status'
import { ComandaPage } from '@/pages/comanda'
import { TableOrdersPage } from '@/pages/table-orders'
import { ProductionPage } from '@/pages/production'

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/menu" element={<MenuPage />} />
      <Route path="/order/:id" element={<OrderStatusPage />} />
      <Route path="/comanda/:tableId/:customerName" element={<ComandaPage />} />
      <Route path="/mesa/:tableId" element={<TableOrdersPage />} />
      <Route path="/producao" element={<ProductionPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
