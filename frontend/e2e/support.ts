import { expect, test, type Page } from '@playwright/test'

export const API = 'http://localhost:5262'

/** In the `real` project, skips the file when the API is not up instead of failing. */
export function skipRealWithoutApi() {
  // Playwright requires the fixtures argument to be destructured, even when no fixture is used.
  // oxlint-disable-next-line no-empty-pattern
  test.beforeAll(async ({}, testInfo) => {
    if (testInfo.project.name !== 'real') return
    const up = await fetch(`${API}/api/dish`).then((r) => r.ok).catch(() => false)
    test.skip(!up, `API fora do ar em ${API}: suba a API na base hamgueria_e2e (CLAUDE.md) para o modo real.`)
  })
}

/** A comanda name no other test uses. */
export const uniqueCustomer = (prefix: string) => `${prefix}-${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`

/** Picks a table, adds the first dish and sends the order under `customer`; ends on the comanda page. */
export async function placeOrder(page: Page, customer: string, table = 'Mesa 01') {
  await page.goto('/menu')
  await page.getByRole('combobox').filter({ hasText: /selecione a mesa|Mesa/ }).click()
  await page.getByRole('option', { name: table }).click()
  await page.getByRole('button', { name: 'Adicionar' }).first().click()
  await page.getByRole('button', { name: 'Enviar pedido' }).click()

  const dialog = page.getByRole('dialog')
  const comandaPicker = dialog.getByRole('combobox')
  if (await comandaPicker.isVisible()) {
    await comandaPicker.click()
    await page.getByRole('option', { name: /nova comanda/ }).click()
  }
  await dialog.getByPlaceholder('ex.: joão').fill(customer)
  await dialog.getByRole('button', { name: 'Enviar pedido' }).click()

  await expect(page.getByRole('heading', { level: 1, name: customer })).toBeVisible()
}

export async function loginKitchen(page: Page, password = 'admin123') {
  await page.goto('/producao')
  await page.getByLabel('usuário').fill('admin')
  await page.getByLabel('senha').fill(password)
  await page.getByRole('button', { name: 'Entrar' }).click()
}
