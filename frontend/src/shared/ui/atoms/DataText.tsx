import type { ReactNode } from 'react'
import { cn } from '@/shared/lib/cn'

/**
 * Tabular data (order number, table, quantity, time, money, counts): the only place the terminal font belongs on
 * work screens, with fixed-width digits so values line up when compared (spec 006, RN-04). Names, notes and labels
 * use the normal font.
 */
export function DataText({ children, className }: { children: ReactNode; className?: string }) {
  return <span className={cn('font-mono tabular-nums', className)}>{children}</span>
}
