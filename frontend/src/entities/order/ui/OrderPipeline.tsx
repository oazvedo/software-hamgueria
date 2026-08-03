import { Check, Loader2, X } from 'lucide-react'
import { getPipeline } from '@/entities/order/model/orderStatus'
import type { PipelineStage, StageState } from '@/entities/order/model/orderStatus'
import type { OrderStatus } from '@/entities/order/model/types'
import { cn } from '@/shared/lib/cn'

function StageIcon({ state, size }: { state: StageState; size: 'sm' | 'md' }) {
  const box = cn(
    'flex shrink-0 items-center justify-center rounded-full border',
    size === 'md' ? 'size-8' : 'size-6',
  )
  const icon = size === 'md' ? 'size-4' : 'size-3'

  if (state === 'complete') {
    return (
      <span className={cn(box, 'border-brand/40 bg-brand/10 text-brand')}>
        <Check className={icon} />
      </span>
    )
  }
  if (state === 'active') {
    return (
      <span className={cn(box, 'border-brand/40 bg-brand/10 text-brand')}>
        <Loader2 className={cn(icon, 'animate-spin')} />
      </span>
    )
  }
  if (state === 'failed') {
    return (
      <span className={cn(box, 'border-destructive/40 bg-destructive/10 text-destructive')}>
        <X className={icon} />
      </span>
    )
  }
  return <span className={cn(box, 'border-border text-muted-foreground/40')}>○</span>
}

/** Deploy-pipeline visualization for an order status (vertical or compact). */
export function OrderPipeline({
  status,
  variant = 'full',
}: {
  status: OrderStatus
  variant?: 'full' | 'compact'
}) {
  const stages = getPipeline(status)

  if (variant === 'compact') {
    return (
      <ol className="flex items-center">
        {stages.map((stage, i, arr) => (
          <li key={stage.key} className="flex flex-1 items-center last:flex-none">
            <StageIcon state={stage.state} size="sm" />
            {i < arr.length - 1 && (
              <span
                className={cn(
                  'mx-1 h-px flex-1',
                  stage.state === 'complete' ? 'bg-brand/40' : 'bg-border',
                )}
              />
            )}
          </li>
        ))}
      </ol>
    )
  }

  return (
    <ol className="flex flex-col">
      {stages.map((stage: PipelineStage, i, arr) => (
        <li key={stage.key} className="flex gap-3">
          <div className="flex flex-col items-center">
            <StageIcon state={stage.state} size="md" />
            {i < arr.length - 1 && (
              <span
                className={cn('w-px flex-1', stage.state === 'complete' ? 'bg-brand/40' : 'bg-border')}
              />
            )}
          </div>
          <div className={cn('pb-6', i === arr.length - 1 && 'pb-0')}>
            <p
              className={cn(
                'font-medium',
                stage.state === 'pending' ? 'text-muted-foreground' : 'text-foreground',
              )}
            >
              {stage.label}
            </p>
            <p className="font-mono text-xs text-muted-foreground">{stage.hint}</p>
          </div>
        </li>
      ))}
    </ol>
  )
}
