import { useState } from 'react'
import { Globe, History } from 'lucide-react'
import { AUDIT_OPERATION_LABEL, AUDIT_TABLE_LABEL, useAuditLog } from '@/entities/audit'
import type { AuditEntry, AuditTable } from '@/entities/audit'
import { DataText } from '@/shared/ui/atoms'
import {
  Button,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Spinner,
} from '@/shared/ui/shadcn'

const ALL = 'all'
const IGNORED = new Set(['UpdatedAt'])

function formatWhen(iso: string): string {
  return new Date(iso).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function readJson(value: string | null): Record<string, unknown> {
  if (!value) return {}
  try {
    return JSON.parse(value) as Record<string, unknown>
  } catch {
    return {}
  }
}

const show = (value: unknown) => (value === null || value === undefined || value === '' ? 'vazio' : String(value))

function screenOf(url: string | null): string | null {
  if (!url) return null
  try {
    return new URL(url).pathname
  } catch {
    return null
  }
}

function Changes({ entry }: { entry: AuditEntry }) {
  if (entry.operation !== 'UPDATE') return null
  const before = readJson(entry.oldValues)
  const after = readJson(entry.newValues)
  const fields = entry.changedFields.filter((f) => !IGNORED.has(f))
  return (
    <ul className="mt-1.5 flex flex-col gap-0.5 text-xs">
      {fields.map((field) => (
        <li key={field} className="text-muted-foreground">
          <span className="text-foreground">{field}</span>: de <DataText>{show(before[field])}</DataText> para{' '}
          <DataText className="text-foreground">{show(after[field])}</DataText>
        </li>
      ))}
    </ul>
  )
}

function Row({ entry }: { entry: AuditEntry }) {
  const screen = screenOf(entry.frontendUrl)
  return (
    <li className="rounded-lg border border-border bg-card px-4 py-3 text-sm">
      <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
        <p className="text-foreground">
          <span className="font-medium">{entry.author}</span> {AUDIT_OPERATION_LABEL[entry.operation]}{' '}
          {AUDIT_TABLE_LABEL[entry.table]}{' '}
          {entry.rowId && <DataText className="text-xs text-muted-foreground">{entry.rowId.slice(0, 8)}</DataText>}
        </p>
        <DataText className="text-xs text-muted-foreground">{formatWhen(entry.createdAt)}</DataText>
      </div>
      <Changes entry={entry} />
      {screen && (
        <p className="mt-1.5 inline-flex items-center gap-1 text-xs text-muted-foreground">
          <Globe aria-hidden="true" className="size-3" />
          pela tela <DataText>{screen}</DataText>
        </p>
      )}
    </li>
  )
}

/** Who changed what, when and from which screen (spec 009). Kitchen only: the API answers 401 otherwise. */
export function AuditLog() {
  const [table, setTable] = useState<AuditTable | typeof ALL>(ALL)
  const { data, isLoading, isError, hasNextPage, fetchNextPage, isFetchingNextPage } = useAuditLog(
    table === ALL ? undefined : table,
  )
  const entries = data?.pages.flatMap((p) => p.items) ?? []

  return (
    <section aria-labelledby="historico-titulo" className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 id="historico-titulo" className="flex items-center gap-2 text-base font-semibold text-foreground">
          <History aria-hidden="true" className="size-4 text-brand" />
          Histórico de mudanças
        </h2>
        <Select value={table} onValueChange={(v) => setTable(v as AuditTable | typeof ALL)}>
          <SelectTrigger className="w-56" aria-label="Filtrar por tipo de registro">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL}>Tudo</SelectItem>
            {(Object.keys(AUDIT_TABLE_LABEL) as AuditTable[]).map((t) => (
              <SelectItem key={t} value={t}>
                {AUDIT_TABLE_LABEL[t].charAt(0).toUpperCase() + AUDIT_TABLE_LABEL[t].slice(1)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading && (
        <div className="flex justify-center p-10">
          <Spinner className="size-6" />
        </div>
      )}
      {isError && (
        <p className="rounded-lg border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Não foi possível carregar o histórico. Sua sessão pode ter expirado: saia e entre de novo.
        </p>
      )}
      {!isLoading && !isError && entries.length === 0 && (
        <p className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
          Nenhuma mudança registrada ainda.
        </p>
      )}

      <ul className="flex flex-col gap-2" aria-label="Mudanças">
        {entries.map((entry) => (
          <Row key={entry.id} entry={entry} />
        ))}
      </ul>

      {hasNextPage && (
        <div className="flex justify-center">
          <Button variant="outline" onClick={() => fetchNextPage()} disabled={isFetchingNextPage}>
            Carregar mais
          </Button>
        </div>
      )}
    </section>
  )
}
