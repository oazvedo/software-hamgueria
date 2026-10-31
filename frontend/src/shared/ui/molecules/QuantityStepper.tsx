import { Minus, Plus } from 'lucide-react'
import { cn } from '@/shared/lib/cn'
import { DataText } from '@/shared/ui/atoms'

interface QuantityStepperProps {
  value: number
  onChange: (value: number) => void
  /** Highest value allowed; the increase button is disabled there. */
  max?: number
  /** Accessible names; default "Diminuir" / "Aumentar". Pass the dish name when several steppers share a page. */
  decreaseLabel?: string
  increaseLabel?: string
  size?: 'sm' | 'md'
}

export function QuantityStepper({
  value,
  onChange,
  max,
  decreaseLabel = 'Diminuir',
  increaseLabel = 'Aumentar',
  size = 'sm',
}: QuantityStepperProps) {
  const atMax = max !== undefined && value >= max
  const button = cn(
    'flex items-center justify-center rounded border border-border text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground disabled:pointer-events-none disabled:opacity-50',
    size === 'sm' ? 'size-6' : 'size-8',
  )
  return (
    <div className="flex items-center gap-1">
      <button type="button" aria-label={decreaseLabel} onClick={() => onChange(value - 1)} className={button}>
        <Minus aria-hidden="true" className="size-3.5" />
      </button>
      <DataText className="w-6 text-center text-foreground">{value}</DataText>
      <button type="button" aria-label={increaseLabel} onClick={() => onChange(value + 1)} disabled={atMax} className={button}>
        <Plus aria-hidden="true" className="size-3.5" />
      </button>
    </div>
  )
}
