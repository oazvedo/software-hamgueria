# 002. Base de testes automatizados: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0005, 0008, 0009, 0010, 0011, 0012, 0013, 0014, 0015
- **ADRs novos propostos:** [0016](../../adr/0016-estrategia-de-testes.md) (estratégia de testes)

## Abordagem
Três suítes, uma por nível (ADR 0016). No servidor, testes de unidade cobrem as regras do domínio e do `OrderService`
com repositórios falsos, e testes funcionais sobem a API real num Postgres descartável (Testcontainers), com o
publicador de eventos trocado por um falso. No frontend, Vitest testa o store do pedido, o JSON e as telas com os
mesmos handlers MSW do app. O Playwright percorre os fluxos cliente e cozinha no navegador, contra os mocks (sempre) e
contra a API real numa base separada (opcional). Nenhum comportamento de produção muda; a única linha de produção nova
é `public partial class Program;`.

## Modelo de dados
Sem mudança. Sem migration. O modo real do e2e usa a base `hamgueria_e2e` no mesmo Postgres do compose (o
`Database.Migrate()` cria a base ao subir a API com essa connection string).

## Contrato da API
Sem mudança. Os testes funcionais exercitam o contrato atual:

| Verbo | Rota | Acesso | Usado em |
|---|---|---|---|
| POST | `/api/auth/login` | Público | CA-09/10 da 001 |
| GET | `/api/dish`, `/api/table` | Público | dados semeados |
| PATCH | `/api/dish/{id}/disable` | Admin | preparar prato inativo (CA-05) |
| POST | `/api/order` | Público | CA-03/04/05 |
| GET | `/api/order/comanda?tableId&customer` | Público | CA-07/08 |
| GET | `/api/order/table/{tableId}` | Público | CA-07 |
| POST | `/api/order/comanda/close` | Público | CA-08 |
| GET | `/api/order` | Admin | CA-09 |
| PATCH | `/api/order/{id}/status`, `/api/order/{id}/cancel` | Admin / Público | CA-11 |

Eventos RabbitMQ: o falso registra as routing keys; um teste confere `order.created` no pedido criado.

## Backend: arquivos
- `backend/src/SoftwareHamgueria.API/Program.cs`: + `public partial class Program;` (fim do arquivo).
- `backend/SoftwareHamgueria.slnx`: pasta `/tests/` com os dois projetos.
- `backend/tests/SoftwareHamgueria.UnitTests/`
  - `SoftwareHamgueria.UnitTests.csproj` (xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, NSubstitute;
    referencia Domain e Application)
  - `Domain/OrderTests.cs`: agrupamento de item repetido (RN-04), status inicial, cancelamento.
  - `Application/OrderServiceTests.cs`: nome de comanda vazio, mesa/prato inexistente, prato inativo (sem gravar nem
    publicar), evento publicado depois de gravar, cálculo de comanda fechada (ADR 0014).
- `backend/tests/SoftwareHamgueria.FunctionalTests/`
  - `SoftwareHamgueria.FunctionalTests.csproj` (xunit.v3, Microsoft.AspNetCore.Mvc.Testing, Testcontainers.PostgreSql;
    referencia API)
  - `Infrastructure/HamgueriaApiFactory.cs`: `WebApplicationFactory<Program>` + container Postgres
    (`IAsyncLifetime`), `UseSetting("ConnectionStrings:Postgres", ...)`, troca `IOrderEventPublisher` por
    `FakeOrderEventPublisher`; coleção xUnit compartilhada (um container por execução).
  - `Infrastructure/FakeOrderEventPublisher.cs`, `Infrastructure/ApiClient.cs` (login admin, helpers de pedido).
  - `IsolationTests.cs`: a connection string em uso não é a de desenvolvimento (CA-05).
  - `OrderEndpointsTests.cs`, `ComandaEndpointsTests.cs`, `AuthEndpointsTests.cs`.

## Frontend: arquivos
- `package.json`: devDependencies `vitest`, `jsdom`, `@testing-library/react`, `@testing-library/dom`,
  `@testing-library/user-event`, `@playwright/test`; scripts `test` (`vitest run`), `test:watch`, `e2e`
  (`playwright test --project=mock`), `e2e:real` (`playwright test --project=real`).
- `vitest.config.ts` (merge do `vite.config.ts`, ambiente jsdom, `setupFiles`, exclui `e2e/`).
- `src/shared/test/setup.ts` (servidor `msw/node` com `handlers`, limpa `localStorage` e handlers entre testes),
  `src/shared/test/renderWithProviders.tsx`, `src/shared/test/index.ts`.
- `src/entities/order/model/orderStore.test.ts`, `orderJson.test.ts`, `orderStatus.test.ts`.
- `src/widgets/order-json/ui/OrderJsonPanel.test.tsx` (CA-02: JSON reflete o carrinho).
- `src/features/submit-order/model/useSubmitOrder.test.tsx` (CA-03: envia, limpa itens, mantém mesa e nome).
- `src/pages/order-status/ui/OrderStatusPage.test.tsx` (CA-06: estágio atual a partir do mock).
- `playwright.config.ts` (projetos `mock` → Vite na 5173 com mocks; `real` → Vite na 5174 com
  `VITE_ENABLE_MOCKS=false`, `test.skip` se `http://localhost:5262/api/dish` não responder).
- `e2e/cliente.spec.ts` (cardápio → pedido → acompanhamento → comanda → fechar comanda; nome de comanda único),
  `e2e/cozinha.spec.ts` (login → finalizar pedido → cliente vê "Servido"; senha errada recusada).
- `.gitignore`: `test-results/`, `playwright-report/`.

Sem handler MSW novo: os testes usam os existentes. Seletores do e2e por papel e texto visível (`getByRole`), sem
`data-testid` novo, salvo se uma tela não tiver nome acessível (vira item de backlog de a11y).

## Processo (CA-07)
- `CLAUDE.md` (seção Testes e Comandos), `implement-task` e `_templates/tasks.md`: a verificação passa a ser
  `dotnet test` / `npm test` filtrado pela parte tocada, e não só build.
- `docs/specs/001-linha-de-base/spec.md`: coluna de cobertura por CA.

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | Unidade + funcional | `dotnet test --solution backend/SoftwareHamgueria.slnx` cobre 001/CA-03, 04, 05, 07, 08, 09, 10, 11; tempo medido no relatório do bloco (≤ 3 min com a imagem já baixada) |
| CA-02 | Vitest | `npm test` cobre 001/CA-02, 03, 06 sem a API; ≤ 1 min |
| CA-03 | Playwright | `npm run e2e` (mock) cobre 001/CA-07, 08, 11, 13; `npm run e2e:real` com a API em `hamgueria_e2e` |
| CA-04 | Manual, uma vez por suíte | Remover a checagem de nome em `OrderService.CreateAsync` → o teste "001/CA-04" fica vermelho com o nome da regra → reverter. Idem no front (não limpar o carrinho no sucesso → "001/CA-03" vermelho) |
| CA-05 | Funcional + manual | `IsolationTests`; e contar pedidos no banco de dev antes e depois do `dotnet test` |
| CA-06 | Execução dupla | Rodar cada suíte duas vezes seguidas; mesmo resultado |
| CA-07 | Revisão de documento | `CLAUDE.md`, `implement-task` e template de tasks atualizados |

## Riscos e decisões pendentes
- **ADR 0016 precisa ser aceito** antes da implementação.
- **Playwright no WSL:** `npx playwright install chromium` e `sudo npx playwright install-deps chromium` (o `sudo`
  é com o usuário). Sem isso o bloco de e2e não roda.
- **Primeira execução lenta:** download da imagem `postgres:16-alpine` pelo Testcontainers; o limite de 3 min vale
  com a imagem em cache.
- **Startup da API nos testes:** o `Program.cs` roda `Migrate()` + seed antes de servir; com a connection string do
  container, isso é o que queremos. O `RabbitMqPublisher` só é resolvido sob demanda, e a troca pelo falso evita a
  conexão.
- **Polling:** telas com `refetchInterval` (2–3 s) deixam o Vitest lento ou instável; os testes usam
  `findBy*` com timeout e `QueryClient` sem retry. Se ficar instável, fake timers.
- **Mock × real:** um e2e que passa no mock e falha no real é achado legítimo (divergência ADR 0013), não flake.
