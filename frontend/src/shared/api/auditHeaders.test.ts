import { afterEach, describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { httpClient } from '@/shared/api/httpClient'
import { setAuditAuthor } from '@/shared/api/auditHeaders'
import { server } from '@/shared/test'

afterEach(() => setAuditAuthor(''))

describe('cabeçalhos da auditoria', () => {
  it('009/CA-02: toda chamada leva o nome da comanda (codificado) e a página aberta', async () => {
    let seen: Headers | undefined
    server.use(http.get('/api/dish', ({ request }) => ((seen = request.headers), HttpResponse.json([]))))
    setAuditAuthor('  João Ção ')

    await httpClient.get('/api/dish')

    expect(decodeURIComponent(seen!.get('X-Author-Name')!)).toBe('João Ção')
    expect(seen!.get('X-Author-Name')).not.toMatch(/[^\x20-\x7e]/)
    expect(seen!.get('X-Frontend-Url')).toBe(window.location.href)
  })

  it('009/CA-02: sem nome de comanda, o cabeçalho de autor não vai', async () => {
    let seen: Headers | undefined
    server.use(http.get('/api/dish', ({ request }) => ((seen = request.headers), HttpResponse.json([]))))

    await httpClient.get('/api/dish')

    expect(seen!.has('X-Author-Name')).toBe(false)
  })
})
