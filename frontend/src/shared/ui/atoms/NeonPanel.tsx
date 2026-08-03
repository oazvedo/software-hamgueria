import type { ReactNode } from 'react'
import { cn } from '@/shared/lib/cn'

/** Neutral surface panel (formerly the neon/glow panel). */
export function NeonPanel({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <div className={cn('rounded-lg border border-border bg-card p-6 shadow-sm', className)}>
      {children}
    </div>
  )
}
