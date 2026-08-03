import type { CreateOrderPayload, OrderDraft } from '@/entities/order/model/types'

// Builds the rich, human-readable JSON shown live in the order panel.
export function buildOrderJson(draft: OrderDraft) {
  return {
    comanda: draft.customerName || null,
    tableId: draft.tableId,
    items: draft.items.map((i) => ({
      dishId: i.dishId,
      dish: i.name,
      category: i.category,
      quantity: i.quantity,
      unitPrice: i.price,
      ...(i.notes.trim() ? { obs: i.notes.trim() } : {}),
    })),
    total: draft.items.reduce((sum, i) => sum + i.price * i.quantity, 0),
  }
}

// Builds the minimal payload the backend expects (CreateOrderDto).
export function buildOrderPayload(draft: OrderDraft): CreateOrderPayload {
  if (!draft.tableId) {
    throw new Error('Selecione uma mesa antes de enviar o pedido.')
  }
  if (!draft.customerName.trim()) {
    throw new Error('Informe o nome da comanda antes de enviar o pedido.')
  }
  return {
    tableId: draft.tableId,
    customerName: draft.customerName.trim(),
    items: draft.items.map((i) => ({
      dishId: i.dishId,
      quantity: i.quantity,
      notes: i.notes.trim() || undefined,
    })),
  }
}
