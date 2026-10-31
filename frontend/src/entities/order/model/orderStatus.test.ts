import { describe, expect, it } from 'vitest'
import { getPipeline, isTerminal } from '@/entities/order/model/orderStatus'

const states = (status: Parameters<typeof getPipeline>[0]) => getPipeline(status).map((s) => s.state)

describe('pipeline do pedido', () => {
  it('001/CA-06: pedido recebido está em produção, com "servido" pendente', () => {
    expect(states('Created')).toEqual(['complete', 'active', 'pending'])
  })

  it('001/CA-06: pedido servido completa o pipeline inteiro', () => {
    expect(states('Done')).toEqual(['complete', 'complete', 'complete'])
  })

  it('001/RN-05: pedido cancelado falha na etapa de produção', () => {
    expect(states('Cancelled')).toEqual(['complete', 'failed', 'pending'])
  })

  it('001/CA-06: o acompanhamento para só em servido ou cancelado', () => {
    expect(isTerminal('Done')).toBe(true)
    expect(isTerminal('Cancelled')).toBe(true)
    expect(isTerminal('Created')).toBe(false)
  })
})
