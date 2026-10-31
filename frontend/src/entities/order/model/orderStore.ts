import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { Dish } from '@/entities/dish/model/types'
import type { OrderItem } from '@/entities/order/model/types'

/** Most units of one dish per order, as the server validates (spec 003, RN-01). */
export const MAX_ITEM_QUANTITY = 20

interface OrderState {
  tableId: string | null
  customerName: string
  items: OrderItem[]
  setTable: (tableId: string | null) => void
  setCustomerName: (customerName: string) => void
  addDish: (dish: Dish) => void
  removeDish: (dishId: string) => void
  setQuantity: (dishId: string, quantity: number) => void
  setNotes: (dishId: string, notes: string) => void
  clear: () => void
}

export const useOrderStore = create<OrderState>()(
  persist(
    (set) => ({
      tableId: null,
      customerName: '',
      items: [],
      // A comanda belongs to a table, so switching tables drops the active
      // comanda name (the checkout modal then offers that table's own comandas).
      setTable: (tableId) =>
        set((state) => (state.tableId === tableId ? { tableId } : { tableId, customerName: '' })),
      setCustomerName: (customerName) => set({ customerName }),
      addDish: (dish) =>
        set((state) => {
          const existing = state.items.find((i) => i.dishId === dish.id)
          if (existing) {
            return {
              items: state.items.map((i) =>
                i.dishId === dish.id ? { ...i, quantity: Math.min(i.quantity + 1, MAX_ITEM_QUANTITY) } : i,
              ),
            }
          }
          return {
            items: [
              ...state.items,
              {
                dishId: dish.id,
                name: dish.name,
                category: dish.category,
                price: dish.price,
                quantity: 1,
                notes: '',
              },
            ],
          }
        }),
      removeDish: (dishId) =>
        set((state) => ({ items: state.items.filter((i) => i.dishId !== dishId) })),
      setQuantity: (dishId, quantity) =>
        set((state) => ({
          items:
            quantity <= 0
              ? state.items.filter((i) => i.dishId !== dishId)
              : state.items.map((i) =>
                  i.dishId === dishId ? { ...i, quantity: Math.min(quantity, MAX_ITEM_QUANTITY) } : i,
                ),
        })),
      setNotes: (dishId, notes) =>
        set((state) => ({
          items: state.items.map((i) => (i.dishId === dishId ? { ...i, notes } : i)),
        })),
      // Clears the cart items but keeps table + comanda name so the next order
      // stays on the same tab (they're part of the persisted session context).
      clear: () => set({ items: [] }),
    }),
    {
      name: 'hamgueria-comanda',
      partialize: (state) => ({ customerName: state.customerName, tableId: state.tableId }),
    },
  ),
)
