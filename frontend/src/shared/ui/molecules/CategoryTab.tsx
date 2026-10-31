import type { LucideIcon } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

interface CategoryTabProps {
  active: boolean
  label: string
  icon?: LucideIcon
  href: string
  onClick?: () => void
}

/** A jump link to a menu section; `active` follows the section on screen (spec 008). */
export function CategoryTab({ active, label, icon: Icon, href, onClick }: CategoryTabProps) {
  return (
    <a
      href={href}
      onClick={onClick}
      aria-current={active ? 'true' : undefined}
      className={cn(
        'inline-flex shrink-0 items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium whitespace-nowrap transition-colors',
        active ? 'bg-background text-foreground shadow-xs' : 'text-muted-foreground hover:text-foreground',
      )}
    >
      {Icon && <Icon aria-hidden="true" className="size-3.5" />}
      {label}
    </a>
  )
}
