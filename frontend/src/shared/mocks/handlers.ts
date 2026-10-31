import { http, HttpResponse } from 'msw'
import { mockDishes, mockTables } from '@/shared/mocks/data'
import {
  hasErrors,
  problems,
  validateComanda,
  validateCreateOrder,
  validateDish,
  validateTable,
  validateUpdateOrder,
  type ItemInput,
} from '@/shared/mocks/validation'

type MockStatus = 'Created' | 'Draft' | 'Cancelled' | 'Done'

interface StoredOrder {
  id: string
  tableId: string
  tableName: string
  customerName: string
  items: Array<{ dishId: string; dishName: string; price: number; quantity: number; notes: string }>
  status: MockStatus
  createdAt: string
  updatedAt: string
}

// localStorage-backed store so mock state is shared across tabs (MSW resolvers
// run per-page, so an in-memory Map wouldn't be visible from the /producao tab).
// Status is driven manually by the kitchen — orders stay 'Created' until an
// admin finalizes or cancels them.
const STORE_KEY = 'hamgueria-mock-orders'

function loadOrders(): StoredOrder[] {
  try {
    return JSON.parse(localStorage.getItem(STORE_KEY) ?? '[]') as StoredOrder[]
  } catch {
    return []
  }
}

function saveOrders(list: StoredOrder[]): void {
  try {
    localStorage.setItem(STORE_KEY, JSON.stringify(list))
  } catch {
    /* ignore storage errors */
  }
}

// Comanda closures: { "<tableId>|<lowercase name>": closedAt ISO }.
const CLOSURES_KEY = 'hamgueria-mock-closures'

function loadClosures(): Record<string, string> {
  try {
    return JSON.parse(localStorage.getItem(CLOSURES_KEY) ?? '{}') as Record<string, string>
  } catch {
    return {}
  }
}

function saveClosures(map: Record<string, string>): void {
  try {
    localStorage.setItem(CLOSURES_KEY, JSON.stringify(map))
  } catch {
    /* ignore storage errors */
  }
}

const closureKey = (tableId: string, name: string) => `${tableId}|${name.toLowerCase()}`

// A comanda is closed for an order if it was closed at/after the order was placed.
function withClosed(order: StoredOrder, closures = loadClosures()) {
  const closedAt = closures[closureKey(order.tableId, order.customerName)]
  const comandaClosed = Boolean(closedAt) && new Date(order.createdAt).getTime() <= new Date(closedAt).getTime()
  return { ...order, comandaClosed }
}

const MOCK_TOKEN = 'mock-kitchen-jwt'

function isAuthed(request: Request): boolean {
  return request.headers.get('Authorization') === `Bearer ${MOCK_TOKEN}`
}

// Same order of checks as OrderService: dish exists, then dish is active (spec 003, CA-09).
function resolveItems(items: ItemInput[]) {
  const resolved: StoredOrder['items'] = []
  for (const it of items) {
    const dish = mockDishes.find((d) => d.id === it.dishId)
    if (!dish) return problems.notFound('O prato informado não existe.')
    if (!dish.isActive) return problems.businessRule(`O prato ${dish.name} não está disponível.`)
    const existing = resolved.find((r) => r.dishId === dish.id)
    if (existing) {
      // Repeated dishes merge into one item (001/RN-04).
      existing.quantity += it.quantity ?? 0
      if (it.notes) existing.notes = it.notes
    } else {
      resolved.push({ dishId: dish.id, dishName: dish.name, price: dish.price, quantity: it.quantity ?? 0, notes: it.notes ?? '' })
    }
  }
  return resolved
}

// Kitchen-only dish and table writes: validated like the server, echoed back (no screen keeps them).
function echoDish(id: string, body: Record<string, unknown>) {
  const now = new Date().toISOString()
  return { ...body, id, name: String(body.name).trim(), isActive: true, createdAt: now, updatedAt: now }
}

function echoTable(id: string, body: { tableName?: string }) {
  const now = new Date().toISOString()
  return { id, tableName: String(body.tableName).trim(), isActive: true, createdAt: now, updatedAt: now }
}

export const handlers = [
  // Same contract as the API's health probe (spec 004): plain text, no details.
  http.get('/api/health', () => HttpResponse.text('Healthy')),
  // Spec 005: database and queue, with the events waiting to be sent. In mock mode everything is up.
  http.get('/api/health/details', () =>
    HttpResponse.json({ status: 'Healthy', database: 'Healthy', queue: 'Healthy', pendingEvents: 0 }),
  ),
  http.get('/api/dish', () => HttpResponse.json(mockDishes)),
  http.get('/api/table', () => HttpResponse.json(mockTables)),

  http.post('/api/auth/login', async ({ request }) => {
    const { username, password } = (await request.json()) as {
      username: string
      password: string
    }
    if (username === 'admin' && password === 'admin123') {
      return HttpResponse.json({
        token: MOCK_TOKEN,
        expiresAt: new Date(Date.now() + 8 * 3600_000).toISOString(),
      })
    }
    return problems.invalidCredentials()
  }),

  http.post('/api/dish', async ({ request }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const body = (await request.json()) as Record<string, unknown>
    const errors = validateDish(body)
    if (hasErrors(errors)) return problems.validation(errors)
    return HttpResponse.json(echoDish(crypto.randomUUID(), body), { status: 201 })
  }),

  http.put('/api/dish/:id', async ({ request, params }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const body = (await request.json()) as Record<string, unknown>
    const errors = validateDish(body)
    if (hasErrors(errors)) return problems.validation(errors)
    if (!mockDishes.some((d) => d.id === params.id)) return problems.notFound('Prato não encontrado.')
    return HttpResponse.json(echoDish(String(params.id), body))
  }),

  http.post('/api/table', async ({ request }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const body = (await request.json()) as { tableName?: string }
    const errors = validateTable(body)
    if (hasErrors(errors)) return problems.validation(errors)
    return HttpResponse.json(echoTable(crypto.randomUUID(), body), { status: 201 })
  }),

  http.put('/api/table/:id', async ({ request, params }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const body = (await request.json()) as { tableName?: string }
    const errors = validateTable(body)
    if (hasErrors(errors)) return problems.validation(errors)
    if (!mockTables.some((t) => t.id === params.id)) return problems.notFound('Mesa não encontrada.')
    return HttpResponse.json(echoTable(String(params.id), body))
  }),

  http.post('/api/order', async ({ request }) => {
    const body = (await request.json()) as { tableId: string; customerName: string; items: ItemInput[] }
    const errors = validateCreateOrder(body)
    if (hasErrors(errors)) return problems.validation(errors)
    const table = mockTables.find((t) => t.id === body.tableId)
    if (!table) return problems.notFound('A mesa informada não existe.')
    const items = resolveItems(body.items)
    if (!Array.isArray(items)) return items
    const now = new Date().toISOString()
    const order: StoredOrder = {
      id: crypto.randomUUID(),
      tableId: body.tableId,
      tableName: table.tableName,
      customerName: body.customerName.trim(),
      items,
      status: 'Created',
      createdAt: now,
      updatedAt: now,
    }
    const list = loadOrders()
    list.push(order)
    saveOrders(list)
    return HttpResponse.json({ ...order, comandaClosed: false }, { status: 201 })
  }),

  // Public: close (settle) a comanda so it leaves the active tabs.
  http.post('/api/order/comanda/close', async ({ request }) => {
    const { tableId, customerName } = (await request.json()) as {
      tableId: string
      customerName: string
    }
    const errors = validateComanda('tableId', 'customerName', tableId, customerName)
    if (hasErrors(errors)) return problems.validation(errors)
    const closures = loadClosures()
    closures[closureKey(tableId, customerName.trim())] = new Date().toISOString()
    saveClosures(closures)
    return new HttpResponse(null, { status: 204 })
  }),

  // Kitchen board — requires the admin token.
  http.get('/api/order', ({ request }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const closures = loadClosures()
    return HttpResponse.json(loadOrders().map((o) => withClosed(o, closures)))
  }),

  http.patch('/api/order/:id/status', async ({ request, params }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return problems.notFound('Pedido não encontrado.')
    order.status = (await request.json()) as MockStatus
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return new HttpResponse(null, { status: 204 })
  }),

  http.patch('/api/order/:id/cancel', ({ params }) => {
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return problems.notFound('Pedido não encontrado.')
    order.status = 'Cancelled'
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return new HttpResponse(null, { status: 204 })
  }),

  // Comanda must be registered before the :id route (MSW matches top-down).
  http.get('/api/order/comanda', ({ request }) => {
    const url = new URL(request.url)
    const tableId = url.searchParams.get('tableId')
    const rawCustomer = url.searchParams.get('customer') ?? ''
    const errors = validateComanda('tableId', 'customer', tableId, rawCustomer)
    if (hasErrors(errors)) return problems.validation(errors)
    const customer = rawCustomer.toLowerCase()
    const closures = loadClosures()
    const list = loadOrders()
      .filter((o) => o.tableId === tableId && o.customerName.toLowerCase() === customer)
      .map((o) => withClosed(o, closures))
    return HttpResponse.json(list)
  }),

  http.get('/api/order/table/:tableId', ({ params }) => {
    const closures = loadClosures()
    const list = loadOrders()
      .filter((o) => o.tableId === params.tableId)
      .map((o) => withClosed(o, closures))
    return HttpResponse.json(list)
  }),

  http.put('/api/order/:id', async ({ request, params }) => {
    const body = (await request.json()) as { items: ItemInput[] }
    const errors = validateUpdateOrder(body)
    if (hasErrors(errors)) return problems.validation(errors)
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return problems.notFound('Pedido não encontrado.')
    const items = resolveItems(body.items)
    if (!Array.isArray(items)) return items
    order.items = items
    order.status = 'Draft'
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return HttpResponse.json({ ...order, comandaClosed: false })
  }),

  http.get('/api/order/:id', ({ params }) => {
    const order = loadOrders().find((o) => o.id === params.id)
    if (!order) return problems.notFound()
    return HttpResponse.json(withClosed(order))
  }),
]
