import { beforeEach, describe, expect, it, vi } from 'vitest'
import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { useOrderStore } from '@/entities/order'
import { CheckoutDialog } from '@/features/submit-order'
import { mockDishes } from '@/shared/mocks/data'
import { renderWithProviders, server } from '@/shared/test'

vi.mock('sonner', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))

const burger = mockDishes[3]

beforeEach(() => {
  useOrderStore.setState({ tableId: 't1', customerName: '', items: [] })
  useOrderStore.getState().addDish(burger)
})

const validationProblem = (errors: Record<string, string[]>) =>
  HttpResponse.json(
    { type: 'https://hamgueria.dev/erros/validacao', title: 'Alguns dados não passaram na checagem.', status: 400, errors },
    { status: 400, headers: { 'Content-Type': 'application/problem+json' } },
  )

async function openAndSend(name: string) {
  renderWithProviders(<CheckoutDialog open onOpenChange={() => {}} />)
  const input = await screen.findByRole('textbox')
  await userEvent.type(input, name)
  await userEvent.click(screen.getByRole('button', { name: /enviar pedido/i }))
  return input
}

describe('checkout com recusa do servidor', () => {
  it('003/CA-11: o motivo do nome da comanda aparece abaixo do campo e o carrinho fica como estava', async () => {
    server.use(http.post('/api/order', () => validationProblem({ customerName: ['Informe o nome da comanda.'] })))

    const input = await openAndSend('Ana')

    const message = await screen.findByText('Informe o nome da comanda.')
    expect(input.getAttribute('aria-invalid')).toBe('true')
    expect(input.getAttribute('aria-describedby')).toBe(message.id)
    expect(useOrderStore.getState().items).toHaveLength(1)
  })

  it('003/CA-11: o motivo de um item aparece com o nome do prato', async () => {
    server.use(
      http.post('/api/order', () =>
        validationProblem({ 'items[0].quantity': ['A quantidade do item 1 vai de 1 a 20.'] }),
      ),
    )

    await openAndSend('Ana')

    const alert = await screen.findByRole('alert')
    expect(within(alert).getByText(`${burger.name}: A quantidade do item 1 vai de 1 a 20.`)).toBeTruthy()
  })

  it('003/CA-11: editar o nome limpa o motivo antigo', async () => {
    server.use(http.post('/api/order', () => validationProblem({ customerName: ['Informe o nome da comanda.'] })))

    const input = await openAndSend('Ana')
    await screen.findByText('Informe o nome da comanda.')
    await userEvent.type(input, 'x')

    expect(screen.queryByText('Informe o nome da comanda.')).toBeNull()
    expect(input.hasAttribute('aria-invalid')).toBe(false)
  })

  it('003/CA-01: pedido válido é enviado sem mostrar erro', async () => {
    await openAndSend('Ana')

    await vi.waitFor(() => expect(useOrderStore.getState().items).toHaveLength(0))
    expect(screen.queryByRole('alert')).toBeNull()
  })
})
