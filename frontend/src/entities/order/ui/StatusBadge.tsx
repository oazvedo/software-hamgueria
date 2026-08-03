import { STATUS_META, isTerminal } from '@/entities/order/model/orderStatus'
import type { StatusTone } from '@/entities/order/model/orderStatus'
import type { OrderStatus } from '@/entities/order/model/types'
import { cn } from '@/shared/lib/cn'

const TONE_BADGE: Record<StatusTone, string> = {
  active: 'border-brand/40 bg-brand/10 text-brand',
  success: 'border-brand/40 bg-brand/10 text-brand',
  muted: 'border-border bg-secondary text-muted-foreground',
  destructive: 'border-destructive/40 bg-destructive/10 text-destructive',
}

export function StatusBadge({
  status,
  className,
}: {
  status: OrderStatus
  className?: string
}) {
  const meta = STATUS_META[status]
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-sm font-medium',
        TONE_BADGE[meta.tone],
        className,
      )}
    >
      {!isTerminal(status) && <span className="size-1.5 animate-pulse rounded-full bg-current" />}
      {meta.label}
    </span>
  )
}
