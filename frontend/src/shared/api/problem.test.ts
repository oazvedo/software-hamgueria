import { describe, expect, it } from 'vitest'
import { AxiosError, AxiosHeaders } from 'axios'
import { fieldErrors, problemMessage, problemTypes, readProblem } from '@/shared/api/problem'

function axiosError(status: number, data: unknown) {
  return new AxiosError('Request failed', 'ERR_BAD_REQUEST', undefined, undefined, {
    status,
    statusText: '',
    headers: {},
    config: { headers: new AxiosHeaders() },
    data,
  })
}

const validation = {
  type: problemTypes.validation,
  title: 'Alguns dados não passaram na checagem.',
  status: 400,
  traceId: '00-abc',
  errors: { customerName: ['Informe o nome da comanda.'], 'items[0].quantity': ['A quantidade do item 1 vai de 1 a 20.'] },
}

describe('leitura do erro do servidor', () => {
  it('003/RN-06: entrada inválida traz a mensagem de cada campo', () => {
    const error = axiosError(400, validation)

    expect(readProblem(error)?.type).toBe(problemTypes.validation)
    expect(fieldErrors(error)).toEqual({
      customerName: 'Informe o nome da comanda.',
      'items[0].quantity': 'A quantidade do item 1 vai de 1 a 20.',
    })
  })

  it('003/RN-07: regra do restaurante mostra o motivo do servidor', () => {
    const error = axiosError(400, {
      type: problemTypes.businessRule,
      title: 'O restaurante não aceitou essa ação.',
      status: 400,
      detail: 'O prato Burger Monolito não está disponível.',
    })

    expect(problemMessage(error, 'genérico')).toBe('O prato Burger Monolito não está disponível.')
    expect(fieldErrors(error)).toEqual({})
  })

  it('003/RN-08: falha inesperada mostra só o título neutro', () => {
    const error = axiosError(500, {
      type: problemTypes.failure,
      title: 'Algo deu errado do nosso lado. Tente de novo.',
      status: 500,
      traceId: '00-def',
    })

    expect(problemMessage(error, 'genérico')).toBe('Algo deu errado do nosso lado. Tente de novo.')
  })

  it('003/RN-07: sem corpo no formato padrão (rede, texto solto) usa o texto da tela', () => {
    expect(problemMessage(new Error('Network Error'), 'Erro ao enviar o pedido.')).toBe('Erro ao enviar o pedido.')
    expect(problemMessage(axiosError(400, 'texto'), 'Erro ao enviar o pedido.')).toBe('Erro ao enviar o pedido.')
  })
})
