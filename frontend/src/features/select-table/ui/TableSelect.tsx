import { useEffect } from 'react'
import { useTables } from '@/entities/table'
import { useOrderStore } from '@/entities/order'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Spinner,
} from '@/shared/ui/shadcn'

export function TableSelect() {
  const { data: tables, isLoading } = useTables()
  const tableId = useOrderStore((s) => s.tableId)
  const setTable = useOrderStore((s) => s.setTable)

  // Self-heal a stale/persisted tableId that no longer exists in the current
  // table list (e.g. a mock id like "t1" left over after switching to the real
  // backend), which would otherwise fail the CreateOrderDto Guid binding (400).
  useEffect(() => {
    if (tables && tableId && !tables.some((t) => t.id === tableId)) {
      setTable(null)
    }
  }, [tables, tableId, setTable])

  if (isLoading) return <Spinner className="size-4" />

  return (
    <Select value={tableId ?? ''} onValueChange={(v) => setTable(v || null)}>
      <SelectTrigger size="sm" className="min-w-[150px]">
        <SelectValue placeholder="selecione a mesa…" />
      </SelectTrigger>
      <SelectContent>
        {tables?.map((t) => (
          <SelectItem key={t.id} value={t.id}>
            {t.tableName}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
