import { Egg, Layers, Leaf, Rocket, Zap, type LucideIcon } from 'lucide-react'
import type { DishCategory } from '@/entities/dish/model/types'

export interface CategoryMeta {
  category: DishCategory
  slug: string
  label: string
  short: string
  tagline: string
  /** Library icon (CLAUDE.md, ajuste 4): no emoji in the interface (spec 008). */
  icon: LucideIcon
}

// Satirical dev-themed mapping over the backend Category enum.
export const CATEGORY_META: Record<DishCategory, CategoryMeta> = {
  Starters: {
    category: 'Starters',
    slug: 'estagiarios',
    label: 'Estagiários',
    short: 'Setup',
    tagline: 'Entradas: baratos, em grande quantidade e servem pra tudo',
    icon: Egg,
  },
  Hamburgers: {
    category: 'Hamburgers',
    slug: 'principais',
    label: 'Monolitos & Microserviços',
    short: 'Build',
    tagline: 'Pratos principais: builds pesados que resolvem o problema',
    icon: Layers,
  },
  Salads: {
    category: 'Salads',
    slug: 'clean-code',
    label: 'Clean Code',
    short: 'Refactor',
    tagline: 'Saladas: leves, limpas, bem indentadas e sem gambiarra',
    icon: Leaf,
  },
  Drinks: {
    category: 'Drinks',
    slug: 'bar',
    label: 'Combustível',
    short: 'Runtime',
    tagline: 'Chopp por senioridade e cafeína pra manter a stack de pé',
    icon: Zap,
  },
  Dessert: {
    category: 'Dessert',
    slug: 'deploy',
    label: 'Deploy em Produção',
    short: 'Deploy',
    tagline: 'Sobremesas: a recompensa doce depois do merge',
    icon: Rocket,
  },
}

export const CATEGORY_ORDER: DishCategory[] = [
  'Starters',
  'Hamburgers',
  'Salads',
  'Drinks',
  'Dessert',
]
