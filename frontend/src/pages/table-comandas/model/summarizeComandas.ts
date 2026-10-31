import { isComandaClosed, isTerminal, orderTotal } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'

export interface ComandaSummary {
  customerName: string
  orders: number
  inProduction: number
  total: number
  closed: boolean
  lastOrderAt: string
}

/** One summary per comanda of a table: open ones first, then the most recently active (spec 007, RN-02). */
export function summarizeComandas(orders: OrderResponse[]): ComandaSummary[] {
  const byName = new Map<string, OrderResponse[]>()
  for (const order of orders) byName.set(order.customerName, [...(byName.get(order.customerName) ?? []), order])

  return [...byName.entries()]
    .map(([customerName, list]) => ({
      customerName,
      orders: list.length,
      inProduction: list.filter((o) => !isTerminal(o.status)).length,
      total: list.reduce((sum, o) => sum + orderTotal(o), 0),
      closed: isComandaClosed(list),
      lastOrderAt: list.map((o) => o.createdAt).sort().at(-1)!,
    }))
    .sort((a, b) => Number(a.closed) - Number(b.closed) || b.lastOrderAt.localeCompare(a.lastOrderAt))
}
