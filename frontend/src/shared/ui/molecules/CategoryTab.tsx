import { cn } from '@/shared/lib/cn'

interface CategoryTabProps {
  active: boolean
  label: string
  icon?: string
  onClick: () => void
}

export function CategoryTab({ active, label, icon, onClick }: CategoryTabProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'inline-flex shrink-0 items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
        active
          ? 'bg-background text-foreground shadow-xs'
          : 'text-muted-foreground hover:text-foreground',
      )}
    >
      {icon && <span className="text-[0.9em] leading-none">{icon}</span>}
      {label}
    </button>
  )
}
