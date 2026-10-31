import { describe, expect, it } from 'vitest'

describe('handlers MSW', () => {
  it('004/CA-09: a verificação de saúde simulada responde saudável como a API', async () => {
    const response = await fetch(new URL('/api/health', location.origin))

    expect(response.status).toBe(200)
    expect(await response.text()).toBe('Healthy')
  })
})
