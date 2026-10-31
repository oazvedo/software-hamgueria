import { useEffect, type ReactNode } from 'react'
import { useOrderStore } from '@/entities/order'
import { setAuditAuthor } from '@/shared/api/auditHeaders'
import { followSystemTheme } from '@/shared/theme'
import { BrowserRouter } from 'react-router-dom'
import { QueryProvider } from '@/app/providers/QueryProvider'
import { ThemeProvider } from '@/app/providers/ThemeProvider'

/** Keeps the audit author (the comanda name) in step with the cart (ADR 0027). */
function AuditAuthorSync() {
  const customerName = useOrderStore((s) => s.customerName)
  useEffect(() => setAuditAuthor(customerName), [customerName])
  return null
}

/** Follows the system theme until the person picks one (spec 014). */
function SystemThemeSync() {
  useEffect(() => followSystemTheme(), [])
  return null
}

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <QueryProvider>
          <AuditAuthorSync />
          <SystemThemeSync />
          {children}
        </QueryProvider>
      </ThemeProvider>
    </BrowserRouter>
  )
}
