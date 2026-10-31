import { useEffect, useMemo, useState } from 'react'
import { CircleAlert } from 'lucide-react'
import { CATEGORY_META, CATEGORY_ORDER, DishCard, useDishes } from '@/entities/dish'
import type { Dish, DishCategory } from '@/entities/dish'
import { AddDishButton } from '@/features/add-dish-to-order'
import { DataText } from '@/shared/ui/atoms'
import { CategoryTab } from '@/shared/ui/molecules'
import { Alert, AlertDescription, AlertTitle, Spinner } from '@/shared/ui/shadcn'

const sectionId = (category: DishCategory) => `cardapio-${CATEGORY_META[category].slug}`

/** Highlights the category whose section is at the top of the screen. No IntersectionObserver (tests): first one. */
function useSectionOnScreen(categories: DishCategory[]) {
  const [current, setCurrent] = useState<DishCategory | undefined>(categories[0])
  const key = categories.join(',')
  useEffect(() => {
    setCurrent(categories[0])
    if (typeof IntersectionObserver === 'undefined') return
    const observer = new IntersectionObserver(
      (entries) => {
        const top = entries.filter((e) => e.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0]
        const category = top?.target.getAttribute('data-category') as DishCategory | null
        if (category) setCurrent(category)
      },
      { rootMargin: '-120px 0px -60% 0px' },
    )
    categories.forEach((c) => {
      const el = document.getElementById(sectionId(c))
      if (el) observer.observe(el)
    })
    return () => observer.disconnect()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key])
  return [current, setCurrent] as const
}

/**
 * The whole menu on one page, by category (spec 008): a sticky row of jump links that follows the section on screen,
 * and only the dishes the kitchen can make (inactive ones stay out).
 */
export function MenuCatalog() {
  const { data: dishes, isLoading, isError } = useDishes()

  const grouped = useMemo(() => {
    const map = new Map<DishCategory, Dish[]>()
    dishes
      ?.filter((d) => d.isActive)
      .forEach((d) => map.set(d.category, [...(map.get(d.category) ?? []), d]))
    return map
  }, [dishes])
  const categories = CATEGORY_ORDER.filter((c) => (grouped.get(c)?.length ?? 0) > 0)
  const [current, setCurrent] = useSectionOnScreen(categories)

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
        <AlertTitle>Não foi possível carregar o cardápio</AlertTitle>
        <AlertDescription>Confira a conexão e recarregue a página.</AlertDescription>
      </Alert>
    )
  }

  if (categories.length === 0) {
    return <p className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">O cardápio está vazio por enquanto.</p>
  }

  return (
    <div className="flex flex-col gap-8">
      <nav aria-label="Categorias do cardápio" className="sticky top-16 z-[5] -mx-6 bg-background px-6 py-2">
        <div className="flex gap-1 overflow-x-auto rounded-lg border border-border bg-secondary/40 p-1 [scrollbar-width:none]">
          {categories.map((cat) => (
            <CategoryTab
              key={cat}
              href={`#${sectionId(cat)}`}
              active={current === cat}
              icon={CATEGORY_META[cat].icon}
              label={CATEGORY_META[cat].short}
              onClick={() => setCurrent(cat)}
            />
          ))}
        </div>
      </nav>

      {categories.map((cat) => {
        const list = grouped.get(cat)!
        const { icon: Icon, label, tagline } = CATEGORY_META[cat]
        return (
          <section key={cat} id={sectionId(cat)} data-category={cat} aria-labelledby={`${sectionId(cat)}-titulo`} className="flex scroll-mt-32 flex-col gap-4">
            <div className="flex items-baseline justify-between gap-3 border-b border-border pb-2">
              <div>
                <h2 id={`${sectionId(cat)}-titulo`} className="flex items-center gap-2 text-lg font-semibold text-foreground">
                  <Icon aria-hidden="true" className="size-4 text-brand" />
                  {label}
                </h2>
                <p className="mt-0.5 text-xs text-muted-foreground">{tagline}</p>
              </div>
              <span className="shrink-0 text-xs text-muted-foreground">
                <DataText>{list.length}</DataText> {list.length === 1 ? 'item' : 'itens'}
              </span>
            </div>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {list.map((dish) => (
                <DishCard key={dish.id} dish={dish} action={<AddDishButton dish={dish} />} />
              ))}
            </div>
          </section>
        )
      })}
    </div>
  )
}
