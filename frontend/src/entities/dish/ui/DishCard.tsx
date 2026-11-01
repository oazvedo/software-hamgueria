import type { ReactNode } from 'react'
import type { Dish } from '@/entities/dish/model/types'
import { DataText } from '@/shared/ui/atoms'
import { formatCurrency } from '@/shared/lib/formatCurrency'

/**
 * One dish of the menu: name and price on the first line, a two-line description, and the action the page passes in
 * (an entity does not import features, ADR 0011). No category chip: the card already sits under its category.
 */
export function DishCard({ dish, action }: { dish: Dish; action?: ReactNode }) {
  return (
    <article className="flex flex-col gap-2 rounded-lg border border-border bg-card p-3.5 transition-colors hover:border-muted-foreground/30">
      <div className="flex items-start justify-between gap-3">
        <h3 className="text-sm font-semibold leading-snug text-card-foreground">{dish.name}</h3>
        <DataText className="shrink-0 text-sm font-semibold text-foreground">{formatCurrency(dish.price)}</DataText>
      </div>
      <p className="line-clamp-2 flex-1 text-xs leading-relaxed text-muted-foreground">{dish.description}</p>
      {action && <div className="flex justify-end">{action}</div>}
    </article>
  )
}
