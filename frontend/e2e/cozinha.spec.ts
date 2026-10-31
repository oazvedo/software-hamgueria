import { expect, test } from '@playwright/test'
import { loginKitchen, placeOrder, skipRealWithoutApi, uniqueCustomer } from './support'

skipRealWithoutApi()

test('001/CA-09: sem login, a tela da cozinha mostra o formulário de acesso', async ({ page }) => {
  await page.goto('/producao')

  await expect(page.getByRole('heading', { name: 'Acesso à produção' })).toBeVisible()
})

test('001/CA-10: senha errada da cozinha é recusada', async ({ page }) => {
  await loginKitchen(page, 'senha-errada')

  await expect(page.getByText('Credenciais inválidas.')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Acesso à produção' })).toBeVisible()
})

test('001/CA-11: a cozinha finaliza o pedido e o cliente vê "Servido"', async ({ page, context }) => {
  const customer = uniqueCustomer('bia')
  await placeOrder(page, customer)

  const kitchen = await context.newPage()
  await loginKitchen(kitchen)
  const card = kitchen
    .getByText(customer, { exact: true })
    .locator('xpath=ancestor::*[.//button[normalize-space()="Finalizar"]][1]')
  await card.getByRole('button', { name: 'Finalizar' }).click()

  await expect(page.getByText('Servido').first()).toBeVisible({ timeout: 10_000 })
})
