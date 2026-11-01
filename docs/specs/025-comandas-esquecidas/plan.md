# 025. Plan

- **ADRs aplicados:** 0006, 0008, 0009, 0013, 0021, 0022. Sem ADR novo.

## Abordagem
`GET /api/order/comandas/idle?hours=6` (Admin): agrupa os pedidos por mesa e nome no banco (`GetComandasIdleSinceAsync`) e
descarta as comandas fechadas depois do último pedido. Validação de `hours` no serviço. No front, `useIdleComandas`
(atualiza a cada minuto) e o widget `idle-comandas` no topo da cozinha, com "Fechar comanda" (`useCloseComanda`, que
passa a invalidar o aviso). Handler MSW com a mesma regra.

## Verificação
CA-01 a CA-03: `IdleComandasTests` (pedido de 3 dias, de 1 h e comanda fechada). CA-01, CA-02, CA-04:
`IdleComandas.test.tsx`. Print no PR.
