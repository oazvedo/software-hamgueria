import { useQuery } from '@tanstack/react-query'
import { httpClient } from '@/shared/api/httpClient'
import { queryKeys } from '@/shared/api/queryKeys'
import type { Dish } from '@/entities/dish/model/types'

// Port: the dish catalog (GET /api/dish). The public menu asks only for active dishes (spec 024).
export async function fetchDishes(onlyActive = false): Promise<Dish[]> {
  const { data } = await httpClient.get<Dish[]>('/api/dish', onlyActive ? { params: { active: true } } : undefined)
  return data
}

export function useDishes({ onlyActive = false }: { onlyActive?: boolean } = {}) {
  return useQuery({ queryKey: [...queryKeys.dishes, onlyActive ? 'active' : 'all'], queryFn: () => fetchDishes(onlyActive) })
}
