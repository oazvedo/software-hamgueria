import type { OrderStatus } from '@/entities/order/model/types'

export type StatusTone = 'active' | 'success' | 'muted' | 'destructive'

export interface StatusMeta {
  label: string
  hint: string
  tone: StatusTone
}

// Friendly, deploy-themed labels for each backend status.
export const STATUS_META: Record<OrderStatus, StatusMeta> = {
  Draft: { label: 'Rascunho', hint: 'pedido ainda em staging', tone: 'muted' },
  Created: { label: 'Em produção', hint: 'buildando na cozinha…', tone: 'active' },
  Done: { label: 'Servido', hint: 'deploy concluído', tone: 'success' },
  Cancelled: { label: 'Cancelado', hint: 'rollback aplicado', tone: 'destructive' },
}

export type StageState = 'complete' | 'active' | 'pending' | 'failed'

export interface PipelineStage {
  key: string
  label: string
  hint: string
  state: StageState
}

const STAGES: Array<{ key: string; label: string; hint: string }> = [
  { key: 'received', label: 'Pedido recebido', hint: 'git commit -m "novo pedido"' },
  { key: 'production', label: 'Em produção', hint: 'building & deploy na cozinha' },
  { key: 'served', label: 'Servido', hint: 'deploy em produção concluído' },
]

// Rank of the stage a status currently sits at.
const STATUS_RANK: Record<OrderStatus, number> = {
  Draft: 0,
  Created: 1,
  Done: 3, // beyond the last stage → everything complete
  Cancelled: 1,
}

/** Derives the deploy pipeline (3 stages) for a given order status. */
export function getPipeline(status: OrderStatus): PipelineStage[] {
  const rank = STATUS_RANK[status]
  const cancelled = status === 'Cancelled'

  return STAGES.map((stage, i) => {
    let state: StageState
    if (cancelled) {
      state = i < rank ? 'complete' : i === rank ? 'failed' : 'pending'
    } else if (i < rank) {
      state = 'complete'
    } else if (i === rank) {
      state = 'active'
    } else {
      state = 'pending'
    }
    return { ...stage, state }
  })
}

export function isTerminal(status: OrderStatus): boolean {
  return status === 'Done' || status === 'Cancelled'
}
