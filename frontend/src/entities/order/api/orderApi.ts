import { useQuery } from '@tanstack/react-query'
import { httpClient } from '@/shared/api/httpClient'
import { queryKeys } from '@/shared/api/queryKeys'
import { ownerTokenOf, saveOwnerToken } from '@/entities/order/model/ownerTokens'
import type { CreateOrderPayload, OrderStatus } from '@/entities/order/model/types'

export interface OrderItemResponse {
  dishId: string
  dishName: string
  price: number
  quantity: number
  notes: string
}

export interface OrderResponse {
  id: string
  tableId: string
  tableName: string
  customerName: string
  items: OrderItemResponse[]
  status: OrderStatus
  comandaClosed: boolean
  createdAt: string
  updatedAt: string
  /** Only in the response that creates the order (ADR 0030). */
  ownerToken?: string
}

export async function createOrder(payload: CreateOrderPayload): Promise<OrderResponse> {
  const { data } = await httpClient.post<OrderResponse>('/api/order', payload)
  if (data.ownerToken) saveOwnerToken(data.id, data.ownerToken)
  return data
}

export async function getOrder(id: string): Promise<OrderResponse> {
  const { data } = await httpClient.get<OrderResponse>(`/api/order/${id}`)
  return data
}

export async function getComanda(tableId: string, customer: string): Promise<OrderResponse[]> {
  const { data } = await httpClient.get<OrderResponse[]>('/api/order/comanda', {
    params: { tableId, customer },
  })
  return data
}

export async function getTableOrders(tableId: string): Promise<OrderResponse[]> {
  const { data } = await httpClient.get<OrderResponse[]>(`/api/order/table/${tableId}`)
  return data
}

// --- Kitchen / admin (require the JWT attached by httpClient) ---

export async function getAllOrders(): Promise<OrderResponse[]> {
  const { data } = await httpClient.get<OrderResponse[]>('/api/order')
  return data
}

export async function updateOrderStatus(id: string, status: OrderStatus): Promise<void> {
  await httpClient.patch(`/api/order/${id}/status`, JSON.stringify(status), {
    headers: { 'Content-Type': 'application/json' },
  })
}

/** Replaces the items of an order this device created (spec 015); the API checks the owner token (ADR 0030). */
export async function updateOrder(id: string, items: Array<{ dishId: string; quantity: number; notes?: string }>): Promise<OrderResponse> {
  const token = ownerTokenOf(id)
  const { data } = await httpClient.put<OrderResponse>(`/api/order/${id}`, { items }, token ? { headers: { 'X-Order-Token': token } } : undefined)
  return data
}

/** The kitchen's token goes by the client; a customer also sends the order's owner token (spec 012). */
export async function cancelOrder(id: string): Promise<void> {
  const token = ownerTokenOf(id)
  await httpClient.patch(`/api/order/${id}/cancel`, undefined, token ? { headers: { 'X-Order-Token': token } } : undefined)
}

export async function closeComanda(tableId: string, customerName: string): Promise<void> {
  await httpClient.post('/api/order/comanda/close', { tableId, customerName })
}

/** A comanda is closed when it has orders and all of them are on the closed side. */
export function isComandaClosed(orders: OrderResponse[]): boolean {
  return orders.length > 0 && orders.every((o) => o.comandaClosed)
}

const TERMINAL: OrderStatus[] = ['Done', 'Cancelled']

function isTerminalStatus(status: OrderStatus | undefined): boolean {
  return Boolean(status && TERMINAL.includes(status))
}

/** Polls a single order while it is still in progress (stops once terminal). */
export function useOrder(id: string | undefined) {
  return useQuery({
    queryKey: queryKeys.order(id ?? ''),
    queryFn: () => getOrder(id as string),
    enabled: Boolean(id),
    refetchInterval: (query) => (isTerminalStatus(query.state.data?.status) ? false : 2000),
  })
}

function listRefetchInterval(list: OrderResponse[] | undefined): number | false {
  if (!list || list.length === 0) return 2000
  return list.every((o) => isTerminalStatus(o.status)) ? false : 2000
}

/** Polls every order in a comanda; stops once all of them are terminal. */
export function useComanda(tableId: string | undefined, customer: string | undefined) {
  return useQuery({
    queryKey: queryKeys.comanda(tableId ?? '', customer ?? ''),
    queryFn: () => getComanda(tableId as string, customer as string),
    enabled: Boolean(tableId && customer),
    refetchInterval: (query) => listRefetchInterval(query.state.data),
  })
}

/** Polls every order at a table (all comandas); stops once all are terminal. */
export function useTableOrders(tableId: string | undefined) {
  return useQuery({
    queryKey: queryKeys.tableOrders(tableId ?? ''),
    queryFn: () => getTableOrders(tableId as string),
    enabled: Boolean(tableId),
    refetchInterval: (query) => listRefetchInterval(query.state.data),
  })
}

/** Distinct OPEN comanda names at a table (for the checkout dropdown). */
export function useTableComandas(tableId: string | undefined, enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.tableComandas(tableId ?? ''),
    queryFn: async () => {
      const orders = await getTableOrders(tableId as string)
      const openNames = new Set(orders.filter((o) => !o.comandaClosed).map((o) => o.customerName))
      return [...openNames].sort((a, b) => a.localeCompare(b))
    },
    enabled: Boolean(tableId) && enabled,
    staleTime: 0,
  })
}

/** Kitchen board: polls every order in the house (admin-only). */
export function useAllOrders(enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.allOrders,
    queryFn: getAllOrders,
    enabled,
    refetchInterval: 3000,
  })
}
