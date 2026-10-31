import { beforeEach, describe, expect, it } from 'vitest'
import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { useOrderStore } from '@/entities/order'
import { MenuCatalog } from '@/widgets/menu-catalog'
import { mockDishes } from '@/shared/mocks/data'
import { renderWithProviders, server } from '@/shared/test'

const EMOJI = /\p{Extended_Pictographic}/u

beforeEach(() => useOrderStore.setState({ tableId: 't1', customerName: '', items: [] }))

describe('cardápio', () => {
  it('008/CA-01: nenhum emoji na tela; categorias com ícone da biblioteca, fora do leitor de tela', async () => {
    renderWithProviders(<MenuCatalog />)

    const nav = await screen.findByRole('navigation', { name: 'Categorias do cardápio' })
    expect(document.body.textContent).not.toMatch(EMOJI)
    for (const link of within(nav).getAllByRole('link')) {
      expect(link.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true')
    }
  })

  it('008/CA-02: cada categoria é um atalho para a sua seção, na mesma página', async () => {
    renderWithProviders(<MenuCatalog />)

    const nav = await screen.findByRole('navigation', { name: 'Categorias do cardápio' })
    const links = within(nav).getAllByRole('link')
    expect(links.length).toBe(5)
    for (const link of links) {
      const target = link.getAttribute('href')!.slice(1)
      expect(document.getElementById(target)?.tagName).toBe('SECTION')
    }
    expect(links[0].getAttribute('aria-current')).toBe('true')
  })

  it('008/CA-03: prato inativo não aparece, e categoria sem prato ativo some', async () => {
    const dishes = mockDishes.map((d) => (d.category === 'Dessert' || d.id === 'd1' ? { ...d, isActive: false } : d))
    server.use(http.get('/api/dish', () => HttpResponse.json(dishes)))

    renderWithProviders(<MenuCatalog />)

    await screen.findByRole('navigation', { name: 'Categorias do cardápio' })
    expect(screen.queryByText(mockDishes[0].name)).toBeNull()
    expect(screen.getByText(mockDishes[1].name)).toBeTruthy()
    expect(screen.queryByRole('link', { name: /Deploy/ })).toBeNull()
  })

  it('008/CA-04: depois de adicionar, o card vira um seletor de quantidade com o nome do prato', async () => {
    renderWithProviders(<MenuCatalog />)
    const dish = mockDishes[1]
    const card = (await screen.findByText(dish.name)).closest('article')!

    await userEvent.click(within(card).getByRole('button', { name: 'Adicionar' }))
    await userEvent.click(within(card).getByRole('button', { name: `Mais um ${dish.name}` }))

    expect(useOrderStore.getState().items.find((i) => i.dishId === dish.id)?.quantity).toBe(2)
    await userEvent.click(within(card).getByRole('button', { name: `Tirar um ${dish.name}` }))
    await userEvent.click(within(card).getByRole('button', { name: `Tirar um ${dish.name}` }))
    expect(useOrderStore.getState().items).toHaveLength(0)
    expect(within(card).getByRole('button', { name: 'Adicionar' })).toBeTruthy()
  })

  it('008/CA-05 (falha): cardápio fora do ar mostra o motivo em português, sem expor a rota', async () => {
    server.use(http.get('/api/dish', () => HttpResponse.json({}, { status: 500 })))

    renderWithProviders(<MenuCatalog />)

    expect(await screen.findByText('Não foi possível carregar o cardápio')).toBeTruthy()
    expect(document.body.textContent).not.toContain('/api/dish')
  })
})
