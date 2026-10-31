import { useInfiniteQuery } from '@tanstack/react-query'
import { httpClient } from '@/shared/api/httpClient'
import { queryKeys } from '@/shared/api/queryKeys'
import type { AuditPage, AuditTable } from '@/entities/audit/model/types'

const PAGE_SIZE = 30

export async function getAuditPage(table: AuditTable | undefined, page: number): Promise<AuditPage> {
  const { data } = await httpClient.get<AuditPage>('/api/audit', {
    params: { table, page, pageSize: PAGE_SIZE },
  })
  return data
}

/** Kitchen history, newest first, one page at a time (Admin). */
export function useAuditLog(table: AuditTable | undefined) {
  return useInfiniteQuery({
    queryKey: queryKeys.audit(table ?? 'all'),
    queryFn: ({ pageParam }) => getAuditPage(table, pageParam),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.hasMore ? last.page + 1 : undefined),
  })
}
