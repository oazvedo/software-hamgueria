import { useQuery } from '@tanstack/react-query'
import { httpClient } from '@/shared/api/httpClient'
import { queryKeys } from '@/shared/api/queryKeys'
import type { Table } from '@/entities/table/model/types'

export async function fetchTables(): Promise<Table[]> {
  const { data } = await httpClient.get<Table[]>('/api/table')
  return data
}

export function useTables() {
  return useQuery({ queryKey: queryKeys.tables, queryFn: fetchTables })
}
