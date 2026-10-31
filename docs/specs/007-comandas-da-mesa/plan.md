# 007. Todas as comandas da mesa num lugar só: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0011, 0012, 0013, 0015, 0016, 0026 (sem paradas)
- **ADRs novos:** nenhum

## Abordagem
Só frontend; a API já devolve os pedidos da mesa (`GET /api/order/table/{id}`, público) com `comandaClosed`. A página
`pages/table-orders` vira `pages/table-comandas`, em `/comandas/:tableId`: lista de resumos por comanda (sem os pedidos;
o detalhe é a página da comanda). `/mesa/:tableId` redireciona. O header troca `~/mesa` por `~/comandas`, visível com a
mesa escolhida; `~/comanda` (a do cliente) continua quando há nome. A página da comanda troca "Ver a mesa toda" por
"Todas as comandas da mesa".

## Arquivos
- `pages/table-comandas/` (novo, substitui `pages/table-orders/`): `TableComandasPage.tsx` + teste, `model/summarizeComandas.ts` + teste.
- `app/router/routes.tsx`: rota nova e redirecionamento.
- `widgets/header/ui/Header.tsx`: `~/comandas`.
- `pages/comanda/ui/ComandaPage.tsx`: link novo.
- `e2e/cliente.spec.ts`: o passo da mesa usa o link novo; `006/CA-01` segue.

## Verificação
| CA | Como |
|---|---|
| CA-01 | Interface: `Header.test.tsx` (mesa sem nome mostra `~/comandas`) |
| CA-02, CA-03, CA-05 | Interface: `TableComandasPage.test.tsx`; unidade: `summarizeComandas.test.ts` (ordem, totais) |
| CA-04, CA-06 | Interface: link de cada comanda; rota `/mesa/t1` renderiza a lista. E2E: passo da mesa |
