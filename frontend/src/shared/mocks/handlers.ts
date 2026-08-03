import { http, HttpResponse } from 'msw'
import { mockDishes, mockTables } from '@/shared/mocks/data'

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

export const handlers = [
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
    return new HttpResponse(null, { status: 401 })
  }),

  http.post('/api/order', async ({ request }) => {
    const body = (await request.json()) as {
      tableId: string
      customerName: string
      items: Array<{ dishId: string; quantity: number; notes?: string }>
    }
    const table = mockTables.find((t) => t.id === body.tableId)
    const items = body.items.map((it) => {
      const dish = mockDishes.find((d) => d.id === it.dishId)
      return {
        dishId: it.dishId,
        dishName: dish?.name ?? 'Item desconhecido',
        price: dish?.price ?? 0,
        quantity: it.quantity,
        notes: it.notes ?? '',
      }
    })
    const now = new Date().toISOString()
    const order: StoredOrder = {
      id: crypto.randomUUID(),
      tableId: body.tableId,
      tableName: table?.tableName ?? 'Mesa desconhecida',
      customerName: body.customerName,
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
    if (!tableId || !customerName?.trim()) return new HttpResponse(null, { status: 400 })
    const closures = loadClosures()
    closures[closureKey(tableId, customerName)] = new Date().toISOString()
    saveClosures(closures)
    return new HttpResponse(null, { status: 204 })
  }),

  // Kitchen board — requires the admin token.
  http.get('/api/order', ({ request }) => {
    if (!isAuthed(request)) return new HttpResponse(null, { status: 401 })
    const closures = loadClosures()
    return HttpResponse.json(loadOrders().map((o) => withClosed(o, closures)))
  }),

  http.patch('/api/order/:id/status', async ({ request, params }) => {
    if (!isAuthed(request)) return new HttpResponse(null, { status: 401 })
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return new HttpResponse(null, { status: 404 })
    order.status = (await request.json()) as MockStatus
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return new HttpResponse(null, { status: 204 })
  }),

  http.patch('/api/order/:id/cancel', ({ params }) => {
    const list = loadOrders()
    const order = list.find((o) => o.id === params.id)
    if (!order) return new HttpResponse(null, { status: 404 })
    order.status = 'Cancelled'
    order.updatedAt = new Date().toISOString()
    saveOrders(list)
    return new HttpResponse(null, { status: 204 })
  }),

  // Comanda must be registered before the :id route (MSW matches top-down).
  http.get('/api/order/comanda', ({ request }) => {
    const url = new URL(request.url)
    const tableId = url.searchParams.get('tableId')
    const customer = (url.searchParams.get('customer') ?? '').toLowerCase()
    if (!tableId || !customer) return new HttpResponse(null, { status: 400 })
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

  http.get('/api/order/:id', ({ params }) => {
    const order = loadOrders().find((o) => o.id === params.id)
    if (!order) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(withClosed(order))
  }),
]
