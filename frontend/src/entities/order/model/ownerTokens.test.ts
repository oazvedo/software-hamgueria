import { describe, expect, it } from 'vitest'
import { cancelOrder, createOrder, getOrder, updateOrderStatus } from '@/entities/order'
import { clearToken, setToken } from '@/shared/auth/authToken'

const order = () => createOrder({ tableId: 't1', customerName: 'Ana', items: [{ dishId: 'd4', quantity: 1 }] })
const status = (e: unknown) => (e as { response?: { status: number } }).response?.status

describe('dono do pedido (spec 012)', () => {
  it('012/CA-01, CA-02: quem criou o pedido neste aparelho cancela com o token guardado', async () => {
    const created = await order()

    await cancelOrder(created.id)

    expect((await getOrder(created.id)).status).toBe('Cancelled')
    expect((await getOrder(created.id)).ownerToken).toBeUndefined()
  })

  it('012/CA-03 (falha): sem o token (outro aparelho), cancelar é negado', async () => {
    const created = await order()
    localStorage.removeItem('hamgueria-owner-tokens')

    const error = await cancelOrder(created.id).catch((e: unknown) => e)

    expect(status(error)).toBe(403)
    expect((await getOrder(created.id)).status).toBe('Created')
  })

  it('012/CA-05 (falha): pedido servido não pode ser cancelado nem voltar de status', async () => {
    const created = await order()
    setToken('mock-kitchen-jwt')
    await updateOrderStatus(created.id, 'Done')

    expect(status(await cancelOrder(created.id).catch((e: unknown) => e))).toBe(400)
    expect(status(await updateOrderStatus(created.id, 'Created').catch((e: unknown) => e))).toBe(400)
    clearToken()
    expect((await getOrder(created.id)).status).toBe('Done')
  })
})
