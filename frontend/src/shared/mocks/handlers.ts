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
// Owner tokens kept apart from the orders, like the API keeps only a hash (ADR 0030): GET never returns them.
const OWNER_KEY = 'hamgueria-mock-owner-tokens'

function loadOwnerTokens(): Record<string, string> {
  try {
    return JSON.parse(localStorage.getItem(OWNER_KEY) ?? '{}') as Record<string, string>
  } catch {
    return {}
  }
}

/** The kitchen, or whoever holds the token returned when the order was created (spec 012). */
function mayChange(request: Request, orderId: string): boolean {
  if (isAuthed(request)) return true
  const token = request.headers.get('X-Order-Token')
  return Boolean(token) && loadOwnerTokens()[orderId] === token
}

// Same moves as Order.CanMoveTo in the API: served and cancelled are final.
const MOVES: Record<MockStatus, MockStatus[]> = {
  Created: ['Draft', 'Done', 'Cancelled'],
  Draft: ['Draft', 'Created', 'Done', 'Cancelled'],
  Done: [],
  Cancelled: [],
}
const FINAL_LABEL: Partial<Record<MockStatus, string>> = { Done: 'já servido', Cancelled: 'cancelado' }
const STATUS_LABEL: Record<MockStatus, string> = { Draft: 'rascunho', Created: 'em produção', Done: 'servido', Cancelled: 'cancelado' }
const FORBIDDEN = 'Só quem fez o pedido (ou a cozinha) pode alterar ou cancelar.'

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

// Audit trail as the API would have it (ADR 0027): one INSERT per order (author = comanda name) and, when the kitchen
// moved it, an UPDATE of Status by "admin". Newest first, filtered by table and paged like GET /api/audit.
function mockAuditEntries() {
  const origin = typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5173'
  const entries = loadOrders().flatMap((o) => {
    const row = { Id: o.id, TableId: o.tableId, CustomerName: o.customerName, Status: 'Created' }
    const created = {
      table: 'Orders', operation: 'INSERT', rowId: o.id, changedFields: [], oldValues: null,
      newValues: JSON.stringify(row), author: o.customerName, frontendUrl: `${origin}/menu`, requestId: null, createdAt: o.createdAt,
    }
    if (o.status === 'Created') return [created]
    const updated = {
      ...created, operation: 'UPDATE', changedFields: ['Status'], oldValues: JSON.stringify(row),
      newValues: JSON.stringify({ ...row, Status: o.status }), author: 'admin', frontendUrl: `${origin}/producao`, createdAt: o.updatedAt,
    }
    return [created, updated]
  })
  return entries
    .sort((a, b) => b.createdAt.localeCompare(a.createdAt))
    .map((e, i, all) => ({ id: all.length - i, ...e }))
}

export const handlers = [
  http.get('/api/audit', ({ request }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const url = new URL(request.url)
    const table = url.searchParams.get('table')
    const page = Number(url.searchParams.get('page') ?? 1)
    const pageSize = Number(url.searchParams.get('pageSize') ?? 50)
    const all = mockAuditEntries().filter((e) => !table || e.table === table)
    const items = all.slice((page - 1) * pageSize, page * pageSize)
    return HttpResponse.json({ items, page, pageSize, hasMore: all.length > page * pageSize })
  }),
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
    const ownerToken = crypto.randomUUID()
    localStorage.setItem(OWNER_KEY, JSON.stringify({ ...loadOwnerTokens(), [order.id]: ownerToken }))
    return HttpResponse.json({ ...order, comandaClosed: false, ownerToken }, { status: 201 })
  }),

  // Public: close (settle) a comanda so it leaves the active tabs.
  http.post('/api/order/comanda/close', async ({ request }) => {
    const { tableId, customerName } = (await request.json()) as {
      tableId: string
      customerName: string
    }
    const errors = validateComanda('tableId', 'customerName', tableId, customerName)
    if (hasErrors(errors)) return problems.validation(errors)
    if (!mockTables.some((t) => t.id === tableId)) return problems.notFound('A mesa informada não existe.')
    const closures = loadClosures()
    closures[closureKey(tableId, customerName.trim())] = new Date().toISOString()
    saveClosures(closures)
    return new HttpResponse(null, { status: 204 })
  }),

  // Kitchen board — requires the admin token.
  http.get('/api/order', ({ request }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const closures = loadClosures()
    // Same bound as the API (spec 020): in progress at any age, finished in the last 24 h, newest first, up to 500.
    const since = Date.now() - 24 * 60 * 60 * 1000
    const visible = loadOrders()
      .filter((o) => !['Done', 'Cancelled'].includes(o.status) || Date.parse(o.updatedAt) >= since)
      .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))
      .slice(0, 500)
    return HttpResponse.json(visible.map((o) => withClosed(o, closures)))
  }),

  http.patch('/api/order/:id/status', async ({ request, params }) => {
    if (!isAuthed(request)) return problems.unauthorized()
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return problems.notFound('Pedido não encontrado.')
    const next = (await request.json()) as MockStatus
    if (!MOVES[order.status].includes(next))
      return problems.businessRule(
        FINAL_LABEL[order.status]
          ? `Um pedido ${FINAL_LABEL[order.status]} não muda mais de status.`
          : `O pedido não pode ir de ${STATUS_LABEL[order.status]} para ${STATUS_LABEL[next]}.`,
      )
    order.status = next
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return new HttpResponse(null, { status: 204 })
  }),

  http.patch('/api/order/:id/cancel', ({ request, params }) => {
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return problems.notFound('Pedido não encontrado.')
    if (!mayChange(request, order.id)) return problems.forbidden(FORBIDDEN)
    if (!MOVES[order.status].includes('Cancelled'))
      return problems.businessRule(`Um pedido ${FINAL_LABEL[order.status]} não pode ser cancelado.`)
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
    if (!mayChange(request, order.id)) return problems.forbidden(FORBIDDEN)
    if (!MOVES[order.status].includes('Draft'))
      return problems.businessRule(`Um pedido ${FINAL_LABEL[order.status]} não pode mais ser alterado.`)
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
