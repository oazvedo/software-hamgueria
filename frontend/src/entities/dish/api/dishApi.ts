import { useQuery } from '@tanstack/react-query'
import { httpClient } from '@/shared/api/httpClient'
import { queryKeys } from '@/shared/api/queryKeys'
import type { Dish } from '@/entities/dish/model/types'

// Port: fetches the full dish catalog from the backend (GET /api/dish).
export async function fetchDishes(): Promise<Dish[]> {
  const { data } = await httpClient.get<Dish[]>('/api/dish')
  return data
}

export function useDishes() {
  return useQuery({ queryKey: queryKeys.dishes, queryFn: fetchDishes })
}
