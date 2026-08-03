import { env } from '@/shared/config/env'

export async function enableMocking(): Promise<void> {
  if (!env.enableMocks) return
  const { worker } = await import('@/shared/mocks/browser')
  await worker.start({ onUnhandledRequest: 'bypass' })
}
