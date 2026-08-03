import type { OrderResponse } from '@/entities/order/api/orderApi'

/** Sum of an order's line totals (price × quantity). */
export function orderTotal(order: OrderResponse): number {
  return order.items.reduce((sum, it) => sum + it.price * it.quantity, 0)
}
