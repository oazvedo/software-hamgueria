interface QuantityStepperProps {
  value: number
  onChange: (value: number) => void
  /** Highest value allowed; "Aumentar" is disabled there. */
  max?: number
}

export function QuantityStepper({ value, onChange, max }: QuantityStepperProps) {
  const atMax = max !== undefined && value >= max
  return (
    <div className="flex items-center gap-1">
      <button
        type="button"
        aria-label="Diminuir"
        onClick={() => onChange(value - 1)}
        className="flex h-6 w-6 items-center justify-center rounded border border-border text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
      >
        −
      </button>
      <span className="w-6 text-center text-foreground">{value}</span>
      <button
        type="button"
        aria-label="Aumentar"
        onClick={() => onChange(value + 1)}
        disabled={atMax}
        className="flex h-6 w-6 items-center justify-center rounded border border-border text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground disabled:pointer-events-none disabled:opacity-50"
      >
        +
      </button>
    </div>
  )
}
