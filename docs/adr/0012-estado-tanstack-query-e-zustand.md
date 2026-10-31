# 0012. Estado: TanStack Query (servidor) e Zustand (cliente)

- **Status:** Aceito
- **Data:** 2026-10-31 (retroativo)

## Contexto
O app tem dois tipos de estado: dados que vêm da API (cardápio, pedidos, comandas) e o pedido sendo montado no
navegador (itens, mesa, nome da comanda), que precisa sobreviver a recarregar a página.

## Decisão
- **Estado do servidor:** TanStack Query. Funções de fetch + hooks `useXxx` em `entities/<x>/api/` usando o
  `httpClient` (axios) de `shared/api`. Toda chave de query vem de `shared/api/queryKeys.ts`.
- **"Ao vivo" é polling:** telas de acompanhamento usam `refetchInterval` (2–3 s) e param quando o pedido chega a um
  status terminal. Não há WebSocket/SSE.
- **Mutações** ficam em `features/<acao>/model/useXxx.ts` com `useMutation`, invalidam as chaves afetadas em
  `onSuccess` e dão feedback com `toast` (sonner) em português.
- **Estado do cliente:** Zustand. O pedido em montagem é o `useOrderStore` (`entities/order/model/orderStore.ts`), com
  `persist` só de `tableId` e `customerName`. A sessão da cozinha é o `useAuthStore` (`features/auth`).
- Não guarde dados da API em store Zustand, nem estado de UI local em Query.

## Alternativas consideradas
- **Só Zustand (ou Redux) com fetch manual:** reimplementaria cache, loading e invalidação.
- **Context API para o pedido:** re-render da árvore inteira a cada item e sem `persist` pronto.

## Consequências
- ✅ Cache, loading e invalidação de graça; estado local simples.
- ⚠️ É preciso lembrar de invalidar todas as chaves afetadas por uma mutação (ex.: fechar comanda invalida 4 chaves).
