interface QuantityStepperProps {
  value: number
  onChange: (value: number) => void
}

export function QuantityStepper({ value, onChange }: QuantityStepperProps) {
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
        className="flex h-6 w-6 items-center justify-center rounded border border-border text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
      >
        +
      </button>
    </div>
  )
}
