# 021. Plan

- **ADRs aplicados:** 0011 (FSD), 0012. Sem ADR novo. `vercel-react-best-practices` → `bundle-dynamic-imports` com `React.lazy`.

## Abordagem
`app/router/routes.tsx`: cada página por `React.lazy` + `Suspense` com indicador. A `ProductionPage` (386 linhas) vira
composição (86): `widgets/kitchen-top-bar`, `widgets/kitchen-queue`, `widgets/kitchen-tables`; `OrderActions` vai
para `features/manage-order`; `waitingLabel` para `entities/order`.

## Resultado medido (`npm run build`)
Chunk inicial: 638 kB para 194 kB (61 kB gzip). Cozinha: chunk próprio de 18 kB; cardápio: 101 kB.

## Verificação
CA-01: build (chunks por página). CA-02: `PageLoading` com `role=status`. CA-03: `ProductionPage.test.tsx`,
`MenuCatalog.test.tsx`, e2e da cozinha e do cliente, sem mudança.
