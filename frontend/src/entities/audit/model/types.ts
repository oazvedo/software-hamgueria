export type AuditTable = 'Orders' | 'OrderItems' | 'Dishes' | 'Tables' | 'ComandaClosures'
export type AuditOperation = 'INSERT' | 'UPDATE' | 'DELETE'

export interface AuditEntry {
  id: number
  table: AuditTable
  operation: AuditOperation
  rowId: string | null
  changedFields: string[]
  oldValues: string | null
  newValues: string | null
  author: string
  frontendUrl: string | null
  requestId: string | null
  createdAt: string
}

export interface AuditPage {
  items: AuditEntry[]
  page: number
  pageSize: number
  hasMore: boolean
}

export const AUDIT_TABLE_LABEL: Record<AuditTable, string> = {
  Orders: 'pedido',
  OrderItems: 'item de pedido',
  Dishes: 'prato',
  Tables: 'mesa',
  ComandaClosures: 'fechamento de comanda',
}

export const AUDIT_OPERATION_LABEL: Record<AuditOperation, string> = {
  INSERT: 'criou',
  UPDATE: 'alterou',
  DELETE: 'removeu',
}
