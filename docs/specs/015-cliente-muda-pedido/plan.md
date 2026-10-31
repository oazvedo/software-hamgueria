# 015. Plan

- **ADRs aplicados:** 0011, 0012, 0013, 0015, 0016, 0030. Sem ADR novo.

## Abordagem
`entities/order`: `updateOrder` (PUT com `X-Order-Token`) e `ownerTokenOf` exportado. Feature nova
`features/change-own-order` (`useChangeOwnOrder` + `OwnOrderActions` com os dois diálogos), na `OrderStatusPage`. O
`DialogContent` base ganhou `p-6` (vinha sem padding; o `CheckoutDialog` passa `p-0` e não muda).

## Verificação
CA-01 a CA-05: `OwnOrderActions.test.tsx` (pelo fluxo da página, contra os handlers MSW). Prints no PR.
