import { describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import { closeComanda, createOrder } from '@/entities/order'
import { ComandaPage } from '@/pages/comanda'
import { renderWithProviders } from '@/shared/test'

describe('comanda', () => {
  it('006/CA-04: comanda fechada mostra a instrução como texto comum', async () => {
    await createOrder({ tableId: 't1', customerName: 'Bia', items: [{ dishId: 'd4', quantity: 1 }] })
    await closeComanda('t1', 'Bia')

    renderWithProviders(<ComandaPage />, { route: '/comanda/t1/Bia', path: '/comanda/:tableId/:customerName' })

    const hint = await screen.findByText('Comanda fechada. Para pedir de novo, abra uma nova pelo cardápio.')
    expect(hint.className).not.toContain('font-mono')
    expect(hint.textContent).not.toMatch(/\/\/|—/)
  })
})
