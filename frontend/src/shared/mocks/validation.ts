import { HttpResponse } from 'msw'
import type { ApiProblem } from '@/shared/api/problem'
import { problemTypes } from '@/shared/api/problem'

// Same rules and messages as the server validators (backend Application/Validators, ADR 0021).
// Keep both in sync: validation.test.ts repeats the server's test cases (ADR 0013).
export const limits = {
  customerName: 60,
  itemNotes: 200,
  itemQuantityMin: 1,
  itemQuantityMax: 20,
  dishName: 200,
  dishDescription: 1000,
  tableName: 100,
} as const

const categories = ['Starters', 'Hamburgers', 'Salads', 'Drinks', 'Dessert']

type Errors = Record<string, string[]>

function add(errors: Errors, field: string, message: string) {
  errors[field] = [...(errors[field] ?? []), message]
}

function requiredTrimmed(errors: Errors, field: string, value: unknown, max: number, empty: string, tooLong: string) {
  const text = typeof value === 'string' ? value.trim() : ''
  if (!text) add(errors, field, empty)
  else if (text.length > max) add(errors, field, tooLong)
}

function comandaName(errors: Errors, field: string, value: unknown) {
  requiredTrimmed(
    errors,
    field,
    value,
    limits.customerName,
    'Informe o nome da comanda.',
    `O nome da comanda pode ter no máximo ${limits.customerName} caracteres.`,
  )
}

function tableChosen(errors: Errors, field: string, value: unknown) {
  if (!value) add(errors, field, 'Escolha uma mesa.')
}

export interface ItemInput {
  dishId?: string
  quantity?: number
  notes?: string | null
}

function orderItems(errors: Errors, items: ItemInput[] | undefined) {
  if (!items || items.length === 0) {
    add(errors, 'items', 'O pedido precisa de pelo menos um item.')
    return
  }
  const totals = new Map<string, number>()
  items.forEach((item, i) => {
    const n = i + 1
    const path = `items[${i}]`
    const quantity = item.quantity ?? 0
    if (!item.dishId) add(errors, `${path}.dishId`, `Escolha o prato do item ${n}.`)
    if (quantity < limits.itemQuantityMin || quantity > limits.itemQuantityMax) {
      add(errors, `${path}.quantity`, `A quantidade do item ${n} vai de ${limits.itemQuantityMin} a ${limits.itemQuantityMax}.`)
    } else if (item.dishId) {
      const total = (totals.get(item.dishId) ?? 0) + quantity
      totals.set(item.dishId, total)
      if (total > limits.itemQuantityMax) {
        add(errors, `${path}.quantity`, `O prato do item ${n} passa de ${limits.itemQuantityMax} unidades somando as repetições.`)
      }
    }
    if ((item.notes?.length ?? 0) > limits.itemNotes) {
      add(errors, `${path}.notes`, `A observação do item ${n} pode ter no máximo ${limits.itemNotes} caracteres.`)
    }
  })
}

export function validateCreateOrder(body: { tableId?: string; customerName?: string; items?: ItemInput[] }): Errors {
  const errors: Errors = {}
  tableChosen(errors, 'tableId', body.tableId)
  comandaName(errors, 'customerName', body.customerName)
  orderItems(errors, body.items)
  return errors
}

export function validateUpdateOrder(body: { items?: ItemInput[] }): Errors {
  const errors: Errors = {}
  orderItems(errors, body.items)
  return errors
}

export function validateComanda(tableField: string, nameField: string, tableId: unknown, name: unknown): Errors {
  const errors: Errors = {}
  tableChosen(errors, tableField, tableId)
  comandaName(errors, nameField, name)
  return errors
}

export function validateDish(body: { name?: string; description?: string; price?: number; category?: string }): Errors {
  const errors: Errors = {}
  requiredTrimmed(
    errors,
    'name',
    body.name,
    limits.dishName,
    'Informe o nome do prato.',
    `O nome do prato pode ter no máximo ${limits.dishName} caracteres.`,
  )
  if ((body.description?.length ?? 0) > limits.dishDescription) {
    add(errors, 'description', `A descrição do prato pode ter no máximo ${limits.dishDescription} caracteres.`)
  }
  if (!(typeof body.price === 'number' && body.price > 0)) {
    add(errors, 'price', 'O preço do prato precisa ser maior que zero.')
  } else if (Math.abs(body.price * 100 - Math.round(body.price * 100)) > 1e-6 || body.price >= 1e8) {
    // Same as the API's numeric(10,2) rule (spec 016): at most 2 decimal places, up to 99.999.999,99.
    add(errors, 'price', 'O preço do prato vai até 99.999.999,99, com no máximo 2 casas decimais.')
  }
  if (!categories.includes(body.category ?? '')) add(errors, 'category', 'Escolha uma categoria do cardápio.')
  return errors
}

export function validateTable(body: { tableName?: string }): Errors {
  const errors: Errors = {}
  requiredTrimmed(
    errors,
    'tableName',
    body.tableName,
    limits.tableName,
    'Informe o nome da mesa.',
    `O nome da mesa pode ter no máximo ${limits.tableName} caracteres.`,
  )
  return errors
}

// ---- ProblemDetails responses (ADR 0022)

const titles = {
  validation: 'Alguns dados não passaram na checagem.',
  businessRule: 'O restaurante não aceitou essa ação.',
  notFound: 'Não encontramos o que você pediu.',
  unauthorized: 'Faça login na cozinha para continuar.',
  invalidCredentials: 'Usuário ou senha inválidos.',
  forbidden: 'Seu login não tem permissão para isso.',
} as const

function problem(body: Omit<ApiProblem, 'traceId'>) {
  return HttpResponse.json({ ...body, traceId: `mock-${crypto.randomUUID()}` } satisfies ApiProblem, {
    status: body.status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

export const problems = {
  validation: (errors: Errors) =>
    problem({ type: problemTypes.validation, title: titles.validation, status: 400, errors }),
  businessRule: (detail: string) =>
    problem({ type: problemTypes.businessRule, title: titles.businessRule, status: 400, detail }),
  notFound: (detail?: string) =>
    problem({ type: problemTypes.notFound, title: titles.notFound, status: 404, ...(detail ? { detail } : {}) }),
  unauthorized: (title: string = titles.unauthorized) =>
    problem({ type: problemTypes.unauthorized, title, status: 401 }),
  invalidCredentials: () => problems.unauthorized(titles.invalidCredentials),
  forbidden: (detail: string) =>
    problem({ type: problemTypes.forbidden, title: titles.forbidden, status: 403, detail }),
}

export const hasErrors = (errors: Errors) => Object.keys(errors).length > 0
