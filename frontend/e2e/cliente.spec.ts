import { expect, test } from '@playwright/test'
import { placeOrder, skipRealWithoutApi, uniqueCustomer } from './support'

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
