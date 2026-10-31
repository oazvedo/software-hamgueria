import { beforeEach, describe, expect, it } from 'vitest'
import { screen } from '@testing-library/react'
import { useOrderStore } from '@/entities/order'
import { Header } from '@/widgets/header'
import { renderWithProviders } from '@/shared/test'

beforeEach(() => useOrderStore.setState({ tableId: null, customerName: '', items: [] }))

describe('header', () => {
  it('007/CA-01: com a mesa escolhida e sem nome, o atalho de comandas aparece', () => {
    useOrderStore.setState({ tableId: 't1' })
    renderWithProviders(<Header />)

    expect(screen.getByRole('link', { name: '~/comandas' }).getAttribute('href')).toBe('/comandas/t1')
    expect(screen.queryByRole('link', { name: '~/comanda' })).toBeNull()
  })

  it('007/CA-01: sem mesa, nenhum atalho de comanda', () => {
    renderWithProviders(<Header />)

    expect(screen.queryByRole('link', { name: '~/comandas' })).toBeNull()
  })

  it('007/CA-03: com mesa e nome, os dois atalhos aparecem', () => {
    useOrderStore.setState({ tableId: 't1', customerName: 'Ana' })
    renderWithProviders(<Header />)

    expect(screen.getByRole('link', { name: '~/comanda' }).getAttribute('href')).toBe('/comanda/t1/Ana')
    expect(screen.getByRole('link', { name: '~/comandas' })).toBeTruthy()
  })
})
