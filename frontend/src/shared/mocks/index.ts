export async function enableMocking(): Promise<void> {
  // Same rule as env.enableMocks, read inline so Vite folds it at build time and a build with
  // VITE_ENABLE_MOCKS=false drops the MSW chunk entirely (spec 004, RN-06).
  if (import.meta.env.VITE_ENABLE_MOCKS === 'false') return
  const { worker } = await import('@/shared/mocks/browser')
  await worker.start({ onUnhandledFrame: 'bypass' })
}
