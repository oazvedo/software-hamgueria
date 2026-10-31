import { beforeEach, describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { httpClient } from '@/shared/api/httpClient'
import { newTraceparent, reportError, reportVital, resetBrowserTelemetry } from '@/shared/telemetry'
import { server } from '@/shared/test'

const W3C = /^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/

beforeEach(() => resetBrowserTelemetry())

function capture(path: string) {
  const bodies: unknown[] = []
  server.use(http.post(path, async ({ request }) => (bodies.push(await request.json()), new HttpResponse(null, { status: 204 }))))
  return bodies
}

describe('telemetria do navegador (spec 023)', () => {
  it('023/CA-01: toda chamada à API leva um traceparent W3C novo', async () => {
    const seen: string[] = []
    server.use(http.get('/api/dish', ({ request }) => (seen.push(request.headers.get('traceparent') ?? ''), HttpResponse.json([]))))

    await httpClient.get('/api/dish')
    await httpClient.get('/api/dish')

    expect(seen).toHaveLength(2)
    for (const t of seen) expect(t).toMatch(W3C)
    expect(seen[0]).not.toBe(seen[1])
    expect(newTraceparent()).toMatch(W3C)
  })

  it('023/CA-02: um erro de JavaScript vai para a API com mensagem, pilha e página', async () => {
    const bodies = capture('/api/telemetry/client-error')

    reportError(new Error('quebrou no cardápio'))

    await expect.poll(() => bodies.length).toBe(1)
    expect(bodies[0]).toMatchObject({ message: 'quebrou no cardápio', page: window.location.href })
    expect((bodies[0] as { stack?: string }).stack).toContain('quebrou no cardápio')
  })

  it('023/CA-03 (falha): uma página com erro em laço manda no máximo 5', async () => {
    const bodies = capture('/api/telemetry/client-error')

    for (let i = 0; i < 20; i++) reportError(new Error(`erro ${i}`))

    await expect.poll(() => bodies.length).toBe(5)
  })

  it('023/CA-04: uma medida de Web Vitals vai para a API', async () => {
    const bodies = capture('/api/telemetry/vitals')

    reportVital({ name: 'LCP', value: 1234.5, rating: 'good' })

    await expect.poll(() => bodies.length).toBe(1)
    expect(bodies[0]).toMatchObject({ name: 'LCP', value: 1234.5, rating: 'good', page: window.location.href })
  })
})
