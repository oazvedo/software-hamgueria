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
