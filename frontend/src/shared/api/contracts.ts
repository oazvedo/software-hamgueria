// Shapes the API returns (its JSON contract), shared by the entities and the MSW mocks. They live in shared because
// both sides need them and a lower layer may not import an entity (ADR 0011, spec 022).

export type DishCategory = 'Starters' | 'Hamburgers' | 'Salads' | 'Drinks' | 'Dessert'

export interface Dish {
  id: string
  name: string
  description: string
  price: number
  category: DishCategory
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface Table {
  id: string
  tableName: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}
