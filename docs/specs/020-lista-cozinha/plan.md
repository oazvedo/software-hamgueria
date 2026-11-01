# 020. Plan

- **ADRs aplicados:** 0006, 0008, 0013. Sem ADR novo.

## Abordagem
`IOrderRepository.GetForKitchenAsync(desde, máximo)` troca o `GetAllAsync`: em andamento (qualquer idade) ou atualizado desde
as últimas 24 h, mais recente primeiro, até 500, com `AsSplitQuery`. Mesma regra no handler MSW. Rótulo "últimas 24 h"
no título de Finalizados.

## Verificação
CA-01, CA-02: `KitchenListTests` (pedidos inseridos com idade) e `handlers.test.ts`. CA-03: `KitchenListTests` (teto).
