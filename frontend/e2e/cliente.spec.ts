import { expect, test } from '@playwright/test'
import { addFirstDish, placeOrder, sendCart, skipRealWithoutApi, uniqueCustomer } from './support'

skipRealWithoutApi()

test('001/CA-07, CA-08, CA-13: cliente pede, vê comanda e mesa, fecha a comanda e reabre com o mesmo nome', async ({ page }) => {
  const customer = uniqueCustomer('ana')

  // pedido → comanda
  await placeOrder(page, customer)
  await expect(page.getByText('1 pedido · total da comanda')).toBeVisible()

  // a mesa mostra a comanda
  await page.getByRole('link', { name: 'Ver a mesa toda' }).click()
  await expect(page.getByText(customer).first()).toBeVisible()
  await page.goBack()

  // fechar a comanda
  await page.getByRole('button', { name: 'Fechar comanda' }).click()
  await expect(page.getByText('Fechada', { exact: true })).toBeVisible()

  // novo pedido com o mesmo nome abre comanda nova
  await placeOrder(page, customer)
  await expect(page.getByText('Fechada', { exact: true })).toBeHidden()
  await expect(page.getByRole('button', { name: 'Fechar comanda' })).toBeVisible()
  await expect(page.getByText('2 pedidos · total da comanda')).toBeVisible()
})

test('003/CA-03, CA-01: o carrinho para em 20 unidades do mesmo prato e o pedido no limite é aceito', async ({ page }) => {
  await addFirstDish(page, 'Mesa 02')

  const more = page.getByRole('button', { name: 'Aumentar' })
  for (let i = 1; i < 20; i++) await more.click()
  await expect(more).toBeDisabled()
  await expect(page.getByText('"quantity": 20')).toBeVisible()

  await sendCart(page, uniqueCustomer('limite'))
})

test('006/CA-01: a página se declara em português do Brasil', async ({ page }) => {
  await page.goto('/')
  await expect(page.locator('html')).toHaveAttribute('lang', 'pt-BR')
})
