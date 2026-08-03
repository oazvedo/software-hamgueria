import { useMemo, useState } from 'react'
import { CircleAlert } from 'lucide-react'
import { CATEGORY_META, CATEGORY_ORDER, DishCard, useDishes } from '@/entities/dish'
import type { Dish, DishCategory } from '@/entities/dish'
import { CategoryTab } from '@/shared/ui/molecules'
import { Alert, AlertDescription, AlertTitle, Spinner } from '@/shared/ui/shadcn'

export function MenuCatalog() {
  const { data: dishes, isLoading, isError } = useDishes()
  const [active, setActive] = useState<DishCategory | 'all'>('all')

  const grouped = useMemo(() => {
    const map = new Map<DishCategory, Dish[]>()
    dishes?.forEach((d) => {
      const list = map.get(d.category) ?? []
      list.push(d)
      map.set(d.category, list)
    })
    return map
  }, [dishes])

  if (isLoading) {
    return (
      <div className="flex justify-center p-10">
        <Spinner className="size-6" />
      </div>
    )
  }

  if (isError) {
    return (
      <Alert variant="destructive">
        <CircleAlert />
        <AlertTitle>Falha ao carregar o cardápio</AlertTitle>
        <AlertDescription>GET /api/dish não respondeu como esperado.</AlertDescription>
      </Alert>
    )
  }

  const visibleCategories = active === 'all' ? CATEGORY_ORDER : [active]

  return (
    <div className="flex flex-col gap-6">
      <div className="sticky top-16 z-[5] -mx-6 bg-background/80 px-6 py-2 backdrop-blur">
        <div className="inline-flex max-w-full flex-wrap gap-1 rounded-lg border border-border bg-secondary/40 p-1">
          <CategoryTab active={active === 'all'} label="Todos" onClick={() => setActive('all')} />
          {CATEGORY_ORDER.map((cat) => (
            <CategoryTab
              key={cat}
              active={active === cat}
              icon={CATEGORY_META[cat].icon}
              label={CATEGORY_META[cat].short}
              onClick={() => setActive(cat)}
            />
          ))}
        </div>
      </div>

      {visibleCategories.map((cat) => {
        const list = grouped.get(cat) ?? []
        if (list.length === 0) return null
        return (
          <section key={cat} className="flex flex-col gap-4">
            <div className="flex items-baseline justify-between gap-3 border-b border-border pb-2">
              <div>
                <h2 className="text-lg font-semibold text-foreground">
                  <span className="mr-2">{CATEGORY_META[cat].icon}</span>
                  {CATEGORY_META[cat].label}
                </h2>
                <p className="mt-0.5 text-xs text-muted-foreground">{CATEGORY_META[cat].tagline}</p>
              </div>
              <span className="shrink-0 font-mono text-xs text-muted-foreground">
                {list.length} {list.length === 1 ? 'item' : 'itens'}
              </span>
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {list.map((dish) => (
                <DishCard key={dish.id} dish={dish} />
              ))}
            </div>
          </section>
        )
      })}
    </div>
  )
}
