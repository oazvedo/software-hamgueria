import { isComandaClosed, isTerminal, orderTotal } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'

export interface ComandaSummary {
  customerName: string
  orders: number
  inProduction: number
  total: number
  closed: boolean
  /** Epoch ms of the latest order: compared as numbers, since .NET timestamps vary in fraction length. */
  lastOrderAt: number
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
      lastOrderAt: Math.max(...list.map((o) => Date.parse(o.createdAt))),
    }))
    .sort((a, b) => Number(a.closed) - Number(b.closed) || b.lastOrderAt - a.lastOrderAt)
}
