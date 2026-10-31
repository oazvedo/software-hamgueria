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
})
