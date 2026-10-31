import { afterEach, describe, expect, it } from 'vitest'
import { mockDishes } from '@/shared/mocks/data'
import { problemTypes, type ApiProblem } from '@/shared/api/problem'

// 003/CA-13: the mocks refuse the same input with the same messages and format as the server.
// The cases and messages below are the ones asserted by the server tests (OrderValidatorsTests,
// DishTableValidatorsTests, OrderEndpointsTests). Change both sides together.

const dish = mockDishes[3] // Burger Monolito
const item = (quantity = 1, notes?: string) => ({ dishId: dish.id, quantity, notes })
const order = (overrides: Record<string, unknown> = {}) => ({ tableId: 't1', customerName: 'Ana', items: [item()], ...overrides })

async function send(method: string, url: string, body: unknown, token?: string) {
  const response = await fetch(`${location.origin}${url}`, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: JSON.stringify(body),
  })
  const text = await response.text()
  return { status: response.status, type: response.headers.get('content-type'), body: text ? (JSON.parse(text) as ApiProblem) : null }
}

async function adminToken() {
  const { body } = await send('POST', '/api/auth/login', { username: 'admin', password: 'admin123' })
  return (body as unknown as { token: string }).token
}

afterEach(() => {
  dish.isActive = true
})

describe('003/CA-13: recusas do modo simulado iguais às do servidor', () => {
  it('003/CA-01: pedido no limite (20 unidades, observação de 200) é criado', async () => {
    expect((await send('POST', '/api/order', order({ items: [item(20, 'x'.repeat(200))] }))).status).toBe(201)
  })

  it('003/CA-02: pedido sem itens', async () => {
    const r = await send('POST', '/api/order', order({ items: [] }))
    expect(r.status).toBe(400)
    expect(r.type).toContain('application/problem+json')
    expect(r.body).toMatchObject({ type: problemTypes.validation, errors: { items: ['O pedido precisa de pelo menos um item.'] } })
    expect(r.body?.traceId).toBeTruthy()
  })

  it.each([0, -1, 21])('003/CA-03: quantidade %i', async (quantity) => {
    const r = await send('POST', '/api/order', order({ items: [item(quantity)] }))
    expect(r.body?.errors).toEqual({ 'items[0].quantity': ['A quantidade do item 1 vai de 1 a 20.'] })
  })

  it('003/CA-03: soma do mesmo prato acima de 20', async () => {
    const r = await send('POST', '/api/order', order({ items: [item(15), item(6)] }))
    expect(Object.keys(r.body?.errors ?? {})).toEqual(['items[1].quantity'])
  })

  it('003/CA-03: alterar pedido com quantidade 0', async () => {
    const r = await send('PUT', '/api/order/qualquer', { items: [item(0)] })
    expect(r.body?.errors).toEqual({ 'items[0].quantity': ['A quantidade do item 1 vai de 1 a 20.'] })
  })

  it('003/CA-04: observação com 201 caracteres', async () => {
    const r = await send('POST', '/api/order', order({ items: [item(1, 'x'.repeat(201))] }))
    expect(r.body?.errors).toEqual({ 'items[0].notes': ['A observação do item 1 pode ter no máximo 200 caracteres.'] })
  })

  it.each([
    ['', 'Informe o nome da comanda.'],
    ['   ', 'Informe o nome da comanda.'],
    ['a'.repeat(61), 'O nome da comanda pode ter no máximo 60 caracteres.'],
  ])('003/CA-05: nome de comanda "%s"', async (customerName, message) => {
    const r = await send('POST', '/api/order', order({ customerName }))
    expect(r.body?.errors).toEqual({ customerName: [message] })
    const close = await send('POST', '/api/order/comanda/close', { tableId: 't1', customerName })
    expect(close.body?.errors).toEqual({ customerName: [message] })
  })

  it('003/CA-05: nome com 60 caracteres é aceito', async () => {
    expect((await send('POST', '/api/order', order({ customerName: 'a'.repeat(60) }))).status).toBe(201)
  })

  it('003/CA-06: dois problemas de uma vez', async () => {
    const r = await send('POST', '/api/order', order({ customerName: '', items: [item(0)] }))
    expect(Object.keys(r.body?.errors ?? {}).sort()).toEqual(['customerName', 'items[0].quantity'])
  })

  it('003/CA-07: prato inválido, com a cozinha autenticada', async () => {
    const token = await adminToken()
    const r = await send('POST', '/api/dish', { name: ' ', description: 'd'.repeat(1001), price: -1, category: 'Drinks' }, token)
    expect(r.body?.errors).toEqual({
      name: ['Informe o nome do prato.'],
      description: ['A descrição do prato pode ter no máximo 1000 caracteres.'],
      price: ['O preço do prato precisa ser maior que zero.'],
    })
    const zero = await send('PUT', `/api/dish/${dish.id}`, { name: 'a'.repeat(201), description: '', price: 0, category: 'X' }, token)
    expect(Object.keys(zero.body?.errors ?? {}).sort()).toEqual(['category', 'name', 'price'])
  })

  it('003/CA-07: cadastrar prato sem login é negado no formato padrão', async () => {
    const r = await send('POST', '/api/dish', { name: 'x', description: '', price: 1, category: 'Drinks' })
    expect(r).toMatchObject({ status: 401, body: { type: problemTypes.unauthorized } })
  })

  it.each([
    ['', 'Informe o nome da mesa.'],
    ['m'.repeat(101), 'O nome da mesa pode ter no máximo 100 caracteres.'],
  ])('003/CA-08: mesa "%s"', async (tableName, message) => {
    const r = await send('POST', '/api/table', { tableName }, await adminToken())
    expect(r.body?.errors).toEqual({ tableName: [message] })
  })

  it('003/CA-09: prato inativo, prato e mesa inexistentes', async () => {
    dish.isActive = false
    const inactive = await send('POST', '/api/order', order())
    expect(inactive).toMatchObject({
      status: 400,
      body: { type: problemTypes.businessRule, detail: 'O prato Burger Monolito não está disponível.' },
    })
    dish.isActive = true

    const unknownDish = await send('POST', '/api/order', order({ items: [{ dishId: 'nope', quantity: 1 }] }))
    expect(unknownDish).toMatchObject({ status: 404, body: { detail: 'O prato informado não existe.' } })

    const unknownTable = await send('POST', '/api/order', order({ tableId: 'nope' }))
    expect(unknownTable).toMatchObject({ status: 404, body: { detail: 'A mesa informada não existe.' } })
  })

  it('001/CA-10: credenciais erradas no formato padrão', async () => {
    const r = await send('POST', '/api/auth/login', { username: 'admin', password: 'errada' })
    expect(r).toMatchObject({ status: 401, body: { title: 'Usuário ou senha inválidos.' } })
  })
})

describe('016: preço no modo simulado com a mesma regra do servidor', () => {
  it('016/CA-01, CA-02: até 2 casas passa; 3 casas é recusado', async () => {
    const { validateDish } = await import('@/shared/mocks/validation')
    const dish = (price: number) => ({ name: 'Burger', description: 'ok', price, category: 'Hamburgers' })

    for (const ok of [0.01, 0.1, 19.99, 34.9, 99999999.99]) expect(validateDish(dish(ok))).toEqual({})
    for (const bad of [19.999, 0.001]) {
      expect(validateDish(dish(bad)).price).toEqual(['O preço do prato vai até 99.999.999,99, com no máximo 2 casas decimais.'])
    }
  })
})
