import type { DishCategory } from '@/shared/api/contracts'

// Mirrors the backend OrderStatus enum (serialized as string).
export type OrderStatus = 'Created' | 'Draft' | 'Cancelled' | 'Done'

export interface OrderItem {
  dishId: string
  name: string
  category: DishCategory
  price: number
  quantity: number
  notes: string
}

export interface OrderDraft {
  tableId: string | null
  customerName: string
  items: OrderItem[]
}

// Payload the backend expects (CreateOrderDto).
export interface CreateOrderPayload {
  tableId: string
  customerName: string
  items: Array<{ dishId: string; quantity: number; notes?: string }>
}

/** What the cart needs to know about a dish; any dish of the menu fits (no import of entities/dish, ADR 0011). */
export interface OrderableDish {
  id: string
  name: string
  price: number
  category: DishCategory
}
