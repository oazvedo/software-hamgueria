# 0013. Mocks de API com MSW ligados por padrão

- **Status:** Aceito
- **Data:** 2026-10-31 (retroativo)

## Contexto
O frontend precisa rodar sem Docker, Postgres e RabbitMQ (para demo de portfólio e para iterar na tela rápido).

## Decisão
- **MSW** no navegador, ligado por padrão (`VITE_ENABLE_MOCKS` diferente de `'false'`). Com `VITE_ENABLE_MOCKS=false`,
  as chamadas vão para a API real pelo proxy do Vite (`/api` → `:5262`).
- Handlers em `shared/mocks/handlers.ts`, dados em `shared/mocks/data.ts`. Estado dos mocks em `localStorage`, para a
  tela da cozinha (`/producao`) e a do cliente enxergarem os mesmos pedidos em abas diferentes.
- **Todo endpoint novo ou alterado no backend tem o handler MSW correspondente na mesma spec**, com os mesmos status
  de sucesso e de erro. Handlers de rotas específicas vêm antes de `/:id` (o MSW casa de cima para baixo).

## Alternativas consideradas
- **json-server ou backend fake separado:** mais um processo para subir.
- **Sem mocks:** o frontend dependeria do Docker para qualquer mudança visual.

## Consequências
- ✅ `npm run dev` funciona sozinho.
- ⚠️ A regra de negócio fica duplicada (ex.: cálculo de `comandaClosed` existe no `OrderService` e no handler). Uma
  divergência só aparece testando com a API real.
