import { describe, expect, it } from 'vitest'

describe('handlers MSW', () => {
  it('004/CA-09: a verificação de saúde simulada responde saudável como a API', async () => {
    const response = await fetch(new URL('/api/health', location.origin))

    expect(response.status).toBe(200)
    expect(await response.text()).toBe('Healthy')
  })

  it('005/CA-07: a saúde detalhada simulada tem o mesmo formato da API', async () => {
    const response = await fetch(new URL('/api/health/details', location.origin))

    expect(response.status).toBe(200)
    expect(await response.json()).toEqual({
      status: 'Healthy',
      database: 'Healthy',
      queue: 'Healthy',
      pendingEvents: 0,
    })
  })
})

describe('020: lista da cozinha limitada no modo simulado', () => {
  it('020/CA-02: finalizado há mais de 24 h some; em andamento antigo continua', async () => {
    const old = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString()
    const order = (id: string, status: string) => ({
      id, tableId: 't1', tableName: 'Mesa 01', customerName: id, items: [], status, createdAt: old, updatedAt: old,
    })
    localStorage.setItem('hamgueria-mock-orders', JSON.stringify([order('ativo-velho', 'Created'), order('servido-velho', 'Done')]))

    const response = await fetch(new URL('/api/order', location.origin), { headers: { Authorization: 'Bearer mock-kitchen-jwt' } })
    const ids = ((await response.json()) as Array<{ id: string }>).map((o) => o.id)

    expect(ids).toEqual(['ativo-velho'])
  })
})
