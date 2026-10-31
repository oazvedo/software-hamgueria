# 002. Base de testes automatizados: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Nesta spec os testes **são** o código; cada task entrega testes que
> passam. Implementar com a skill `implement-task`, a spec inteira de uma vez.
> Pré-requisito do backend: Docker rodando. O `dotnet test` usa o Microsoft.Testing.Platform (`global.json` na raiz):
> `--solution`/`--project` em vez do caminho posicional e `--filter-class` em vez de `--filter`.

## Fase 1: servidor
- [x] **T01. Projeto de unidade + regras do pedido no domínio**
  - Arquivos: `backend/tests/SoftwareHamgueria.UnitTests/SoftwareHamgueria.UnitTests.csproj`,
    `backend/tests/SoftwareHamgueria.UnitTests/Domain/OrderTests.cs`, `backend/SoftwareHamgueria.slnx`
  - Cobre: CA-01 (001/RN-04 item repetido somado, RN-05 status inicial e cancelamento)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests` → verde

- [x] **T02. Regras do `OrderService` com repositórios falsos**
  - Arquivos: `backend/tests/SoftwareHamgueria.UnitTests/Application/OrderServiceTests.cs`
  - Cobre: CA-01 (001/CA-04 nome vazio, 001/CA-05 prato inativo e inexistente, sem gravar nem publicar; evento só
    depois de gravar; 001/CA-08 cálculo de comanda fechada), CA-04 (quebrar a checagem de nome em
    `OrderService.CreateAsync` → "001/CA-04" vermelho com o nome da regra → reverter)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.OrderServiceTests"` → verde

- [x] **T03. Infra funcional: API em Postgres descartável + isolamento**
  - Arquivos: `backend/src/SoftwareHamgueria.API/Program.cs` (+ `public partial class Program;`),
    `backend/tests/SoftwareHamgueria.FunctionalTests/SoftwareHamgueria.FunctionalTests.csproj`,
    `.../Infrastructure/HamgueriaApiFactory.cs`, `.../Infrastructure/FakeOrderEventPublisher.cs`,
    `.../Infrastructure/ApiClient.cs`, `.../IsolationTests.cs`, `backend/SoftwareHamgueria.slnx`
  - Cobre: CA-05 (connection string em uso ≠ dev; contar pedidos do banco de dev antes/depois), smoke `GET /api/dish`
    devolve os pratos semeados
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.IsolationTests"` → verde

- [x] **T04. Endpoints de pedido**
  - Arquivos: `backend/tests/SoftwareHamgueria.FunctionalTests/OrderEndpointsTests.cs`
  - Cobre: CA-01 (001/CA-03 201 + status Created + `order.created` publicado; 001/CA-04 400 sem nome;
    001/CA-05 400 prato inativo, 404 prato e mesa inexistentes)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.OrderEndpointsTests"` → verde

- [x] **T05. Endpoints de comanda**
  - Arquivos: `backend/tests/SoftwareHamgueria.FunctionalTests/ComandaEndpointsTests.cs`
  - Cobre: CA-01 (001/CA-07 comanda reúne os pedidos sem diferenciar maiúsculas, mesa lista todas as comandas;
    001/CA-08 fechar marca `comandaClosed`, pedido novo com o mesmo nome abre comanda nova; falha: fechar sem nome → 400)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.ComandaEndpointsTests"` → verde

- [x] **T06. Acesso da cozinha**
  - Arquivos: `backend/tests/SoftwareHamgueria.FunctionalTests/AuthEndpointsTests.cs`
  - Cobre: CA-01 (001/CA-09 `GET /api/order` sem token → 401; 001/CA-10 senha errada → 401, certa → token;
    001/CA-11 admin muda status para Done e o pedido reflete; status sem token → 401)
  - Verificação: `dotnet test --solution backend/SoftwareHamgueria.slnx` → verde, duas vezes seguidas (CA-06), tempo ≤ 3 min
    com a imagem em cache (CA-01)

## Fase 2: interface
- [x] **T07. Vitest configurado + status do pedido**
  - Arquivos: `frontend/package.json` (devDeps + scripts `test`, `test:watch`), `frontend/vitest.config.ts`,
    `frontend/src/shared/test/{setup.ts,renderWithProviders.tsx,index.ts}`,
    `frontend/src/entities/order/model/orderStatus.test.ts`
  - Cobre: CA-02 (base), 001/CA-06 (pipeline e status terminal)
  - Verificação: `cd frontend && npm test -- orderStatus` → verde; `npm run build && npm run lint` → sem erro novo

- [x] **T08. Carrinho e JSON do pedido**
  - Arquivos: `frontend/src/entities/order/model/orderStore.test.ts`,
    `frontend/src/entities/order/model/orderJson.test.ts`
  - Cobre: CA-02 (001/CA-02 adicionar, quantidade, observação, remover com quantidade 0; 001/CA-03 `clear` mantém
    mesa e nome; trocar de mesa limpa o nome)
  - Verificação: `cd frontend && npm test -- orderStore orderJson` → verde

- [x] **T09. Painel JSON reflete o carrinho**
  - Arquivos: `frontend/src/widgets/order-json/ui/OrderJsonPanel.test.tsx`
  - Cobre: CA-02 (001/CA-02 o painel mostra o item e a quantidade novos sem recarregar)
  - Verificação: `cd frontend && npm test -- OrderJsonPanel` → verde

- [x] **T10. Envio do pedido**
  - Arquivos: `frontend/src/features/submit-order/model/useSubmitOrder.test.tsx`
  - Cobre: CA-02 (001/CA-03 envia via mock, esvazia itens, mantém mesa e nome; falha: com o handler sobrescrito no
    teste para responder 400, o carrinho é mantido e aparece o toast de erro), CA-04 (não limpar o carrinho no sucesso → "001/CA-03" vermelho → reverter)
  - Verificação: `cd frontend && npm test -- useSubmitOrder` → verde

- [x] **T11. Tela de acompanhamento**
  - Arquivos: `frontend/src/pages/order-status/ui/OrderStatusPage.test.tsx`
  - Cobre: CA-02 (001/CA-06 a tela mostra o estágio do pedido criado no mock; falha: id inexistente mostra "Pedido não encontrado")
  - Verificação: `cd frontend && npm test` → verde, duas vezes seguidas (CA-06), em ≤ 1 min (CA-02)

## Fase 3: ponta a ponta
- [x] **T12. Playwright configurado + login recusado**
  - Arquivos: `frontend/package.json` (devDep + scripts `e2e`, `e2e:real`), `frontend/playwright.config.ts`,
    `frontend/.gitignore`, `frontend/e2e/cozinha.spec.ts` (só o caso de senha errada)
  - Cobre: CA-03 (base), 001/CA-10 no navegador
  - Pré-requisito do usuário: `npx playwright install chromium` e `sudo npx playwright install-deps chromium`
  - Verificação: `cd frontend && npm run e2e` → verde

- [x] **T13. Fluxo do cliente**
  - Arquivos: `frontend/e2e/cliente.spec.ts`
  - Cobre: CA-03 (001/CA-07 comanda e mesa, 001/CA-08 fechar e reabrir com o mesmo nome, 001/CA-13 tudo com mocks)
  - Verificação: `cd frontend && npm run e2e -- cliente` → verde

- [x] **T14. Fluxo da cozinha**
  - Arquivos: `frontend/e2e/cozinha.spec.ts`
  - Cobre: CA-03 (001/CA-09 sem login vê o formulário, 001/CA-11 cozinha finaliza e o cliente vê "Servido")
  - Verificação: `cd frontend && npm run e2e` → verde, duas vezes seguidas (CA-06)

- [x] **T15. Modo real**
  - Arquivos: `frontend/playwright.config.ts` (projeto `real` + guarda de API no ar), `CLAUDE.md` (comando para subir
    a API na base `hamgueria_e2e`)
  - Cobre: CA-03 (os mesmos fluxos contra a API real), CA-05 (base separada)
  - Verificação: sem API → `npm run e2e:real` pula com mensagem; com a API em `hamgueria_e2e` → verde

## Fase 4: processo
- [x] **T16. Testes viram a verificação padrão**
  - Arquivos: `CLAUDE.md` (Testes, Comandos), `.claude/skills/implement-task/SKILL.md`,
    `.claude/skills/dotnet-feature/SKILL.md`, `.claude/skills/react-feature/SKILL.md`,
    `docs/specs/_templates/tasks.md`
  - Cobre: CA-07
  - Verificação: os quatro comandos de teste aparecem como verificação nos três lugares; nenhuma menção restante a
    "build como verificação mínima"

- [x] **T17. Cobertura na linha de base**
  - Arquivos: `docs/specs/001-linha-de-base/spec.md`
  - Cobre: RN-01 (cada CA da 001 aponta para o teste que o cobre; CA-01 e CA-12 marcados "sem checagem: spec própria")
  - Verificação: todos os CA da 001 têm uma linha na tabela; os nomes batem com `DisplayName`/`it(...)` existentes

## Backlog
<!-- Achados da simplify e da revisão que ficam fora do escopo desta spec. Um item por linha, com o motivo. -->
- **Seed não é seguro com duas instâncias subindo juntas** (Fase 1, simplify/altitude): `DatabaseSeeder` faz
  `if (Dishes.Any()) return;` e insere, sem índice único em nome de prato ou mesa e sem lock. Os testes mostraram o
  efeito: 4 hosts subindo ao mesmo tempo geraram 32 mesas em vez de 8. Em produção, um deploy com réplicas faria o mesmo.
  Correção: seed idempotente (`HasData` em migration, ou índice único + inserir-se-faltar, ou advisory lock). Produção, ADR 0005.
- **Tempo não é injetável** (Fase 1, simplify/altitude): as entidades carimbam `DateTime.UtcNow` direto, e a regra de
  comanda fechada (`CreatedAt <= ClosedAt`) trata um empate no mesmo tick como "fechada". O teste usa `SpinWait` para
  contornar. Correção: `TimeProvider` injetado + `FakeTimeProvider` nos testes. Produção, ADRs 0004 e 0014.
- **Pacotes transitivos com vulnerabilidade conhecida** (Fase 1, build): `System.Security.Cryptography.Xml` 9.0.0 (alta,
  NU1903) na Infrastructure e `System.Security.Cryptography.Pkcs` 9.0.0 (moderada, NU1902) na API. Já existiam antes da
  spec. Correção: subir os pacotes que os trazem ou fixar a versão corrigida.
- **CA-05: contagem manual no banco de dev pendente** (Fase 1): a porta 5432 está ocupada por um Postgres de outro projeto
  (`sdf-manager-postgres`), e o banco do Hamgueria não está de pé. O isolamento está coberto pelo teste automático
  `002/CA-05`; a contagem antes/depois fica para quando o `docker compose` do Hamgueria subir.
- Não feito na simplify, por custo maior que o ganho: cachear o id da mesa e do prato semeados na factory (economiza
  ~30 requisições, < 1% do tempo da suíte).
