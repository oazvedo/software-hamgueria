import { useEffect, type ReactNode } from 'react'
import { useOrderStore } from '@/entities/order'
import { setAuditAuthor } from '@/shared/api/auditHeaders'
import { BrowserRouter } from 'react-router-dom'
import { QueryProvider } from '@/app/providers/QueryProvider'
import { ThemeProvider } from '@/app/providers/ThemeProvider'

/** Keeps the audit author (the comanda name) in step with the cart (ADR 0027). */
function AuditAuthorSync() {
  const customerName = useOrderStore((s) => s.customerName)
  useEffect(() => setAuditAuthor(customerName), [customerName])
  return null
}

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <QueryProvider>
          <AuditAuthorSync />
          {children}
        </QueryProvider>
      </ThemeProvider>
    </BrowserRouter>
  )
}
