# 012. Pedido seguro: Plan

- **Spec:** [spec.md](spec.md) · **ADR novo:** [0030](../../adr/0030-dono-do-pedido-por-token.md) (substitui em parte o 0009)

## Abordagem
Domínio: `Order.OwnerTokenHash`, `CanMoveTo`, guardas em `UpdateStatus`/`Cancel`. Application: `OrderOwnerToken`
(gera, faz hash, compara em tempo constante), `OrderActor` (cozinha ou cliente com token), `ForbiddenException` (403).
`OrderService.CreateAsync` devolve `ownerToken`; `UpdateAsync`/`CancelAsync` recebem o ator; `UpdateStatusAsync` recusa
transição inválida. API: cabeçalho `X-Order-Token`, handler mapeia 403. Migration `AddOrderOwnerToken` (coluna
anulável). Front: `ownerTokens` no `localStorage`, `cancelOrder` manda o cabeçalho; handlers MSW com as mesmas regras.

## Verificação
| CA | Como |
|---|---|
| CA-01 a CA-03, CA-05 | Funcional: `OrderOwnershipTests`; interface: `ownerTokens.test.ts` |
| CA-04, CA-05 | Unidade: `OrderStatusMovesTests` |
