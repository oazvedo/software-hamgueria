import { beforeEach, describe, expect, it } from 'vitest'
import { act, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useOrderStore } from '@/entities/order'
import { OrderJsonPanel } from '@/widgets/order-json'
import { mockDishes } from '@/shared/mocks/data'
import { renderWithProviders } from '@/shared/test'

const burger = mockDishes[3]
const jsonText = () => document.querySelector('pre')?.textContent ?? ''

beforeEach(() => useOrderStore.setState({ tableId: 't1', customerName: '', items: [] }))

describe('painel JSON', () => {
  it('001/CA-02: o JSON reflete na hora o prato adicionado e a quantidade alterada', async () => {
    renderWithProviders(<OrderJsonPanel />)
    expect(jsonText()).not.toContain(burger.name)

    act(() => useOrderStore.getState().addDish(burger))
    expect(jsonText()).toContain(`"dish": "${burger.name}"`)
    expect(jsonText()).toContain('"quantity": 1')

    await userEvent.click(screen.getByRole('button', { name: 'Aumentar' }))
    expect(jsonText()).toContain('"quantity": 2')
  })

  it('001/CA-02: a observação digitada aparece no JSON', async () => {
    useOrderStore.setState({ items: [] })
    act(() => useOrderStore.getState().addDish(burger))
    renderWithProviders(<OrderJsonPanel />)

    await userEvent.type(screen.getByPlaceholderText(/observações/), 'sem cebola')

    expect(jsonText()).toContain('"obs": "sem cebola"')
  })

  it('003/CA-03: o botão Aumentar para em 20 unidades', async () => {
    act(() => useOrderStore.getState().addDish(burger))
    act(() => useOrderStore.getState().setQuantity(burger.id, 19))
    renderWithProviders(<OrderJsonPanel />)
    const more = screen.getByRole('button', { name: 'Aumentar' }) as HTMLButtonElement

    await userEvent.click(more)

    expect(jsonText()).toContain('"quantity": 20')
    expect(more.disabled).toBe(true)
  })

  it('003/CA-03: adicionar o mesmo prato pelo cardápio não passa de 20', () => {
    for (let i = 0; i < 25; i++) act(() => useOrderStore.getState().addDish(burger))

    expect(useOrderStore.getState().items[0].quantity).toBe(20)
  })

  it('006/CA-04: sem mesa escolhida, a instrução aparece como texto comum', () => {
    useOrderStore.setState({ tableId: null })
    act(() => useOrderStore.getState().addDish(burger))
    renderWithProviders(<OrderJsonPanel />)

    const hint = screen.getByText('Selecione uma mesa para enviar o pedido.')
    expect(hint.className).not.toContain('font-mono')
    expect(hint.textContent).not.toMatch(/^\s*\/\//)
  })

  it('006/CA-10: as bolinhas da janela usam tokens da paleta e ficam fora do leitor de tela', () => {
    renderWithProviders(<OrderJsonPanel />)

    const dots = screen.getByTestId('window-dots')
    expect(dots.getAttribute('aria-hidden')).toBe('true')
    expect(dots.innerHTML).not.toMatch(/#[0-9a-f]{3,6}/i)
    expect([...dots.children].map((d) => d.className.match(/bg-window-\w+/)?.[0])).toEqual([
      'bg-window-close',
      'bg-window-minimize',
      'bg-window-zoom',
    ])
  })
})
