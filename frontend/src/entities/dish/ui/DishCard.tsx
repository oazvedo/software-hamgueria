import { AddDishButton } from '@/features/add-dish-to-order'
import { CATEGORY_META } from '@/entities/dish/model/categories'
import type { Dish } from '@/entities/dish/model/types'
import { formatCurrency } from '@/shared/lib/formatCurrency'

export function DishCard({ dish }: { dish: Dish }) {
  const meta = CATEGORY_META[dish.category]

  return (
    <article className="flex flex-col rounded-lg border border-border bg-card p-3.5 transition-colors hover:border-muted-foreground/30">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <h3 className="text-sm font-semibold leading-tight text-card-foreground">{dish.name}</h3>
          <span className="mt-1 inline-flex items-center gap-1 text-[11px] text-muted-foreground">
            <span>{meta.icon}</span>
            {meta.short}
          </span>
        </div>
        <span className="shrink-0 font-mono text-sm font-semibold tracking-tight text-foreground">
          {formatCurrency(dish.price)}
        </span>
      </div>

      <p className="mt-2 line-clamp-2 flex-1 text-xs leading-relaxed text-muted-foreground">
        {dish.description}
      </p>

      <AddDishButton dish={dish} className="mt-3 w-full" />
    </article>
  )
}
