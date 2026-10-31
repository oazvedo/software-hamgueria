# 003. Validação de entrada e erros padronizados: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, a spec inteira de uma vez.
> **Pré-requisito:** ADRs 0021 e 0022 aceitos (2026-10-31). Backend: Docker rodando. O `dotnet test` usa o
> Microsoft.Testing.Platform (`--project` e `--filter-class`).

## Fase 0: decisões
- [x] **T01. Aplicar os ADRs aceitos nas referências**
  - Arquivos: `docs/adr/0008-api-rest-com-controllers.md` (status: substituído em parte por 0022),
    `.claude/skills/dotnet-feature/SKILL.md` (passo 7: validador + exceções próprias; passo 9: controller sem
    `try/catch`; checklist), `CLAUDE.md` (Regras rápidas: validação e erros)
  - Cobre: pré-requisito de todas
  - Verificação: `grep -n "new KeyNotFoundException\|catch (KeyNotFoundException\|new InvalidOperationException" .claude/skills/dotnet-feature/SKILL.md` → vazio

## Fase 1: servidor
- [x] **T02. Limites, exceções próprias e validadores do pedido**
  - Arquivos: `Application/SoftwareHamgueria.Application.csproj` (+ FluentValidation, versão exata),
    `Application/Exceptions/{NotFoundException,BusinessRuleException}.cs`, `Application/Validators/Limits.cs`,
    `Application/Validators/Order/{CreateOrderDto,UpdateOrderDto,CloseComandaDto}Validator.cs`, `OrderItemsRules.cs`
    (regras de item com o número do item na mensagem), `Validators/RuleExtensions.cs`,
    `Application/Validators/Order/ComandaQuery{,Validator}.cs`, `Application/ApplicationModule.cs`,
    `UnitTests/Application/Validators/OrderValidatorsTests.cs`
  - Cobre: CA-01, CA-02, CA-03 (0, -1, 21, soma 15+6), CA-04 (200 aceita, 201 recusa), CA-05 (vazio, espaços, 60, 61),
    CA-06 (dois erros juntos)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.OrderValidatorsTests"` → verde

- [x] **T03. Validadores de prato e mesa**
  - Arquivos: `Application/Validators/Dish/{Create,Update}DishDtoValidator.cs`,
    `Application/Validators/Table/{Create,Update}TableDtoValidator.cs`,
    `UnitTests/Application/Validators/{Dish,Table}ValidatorsTests.cs`
  - Cobre: CA-07 (nome vazio e 201, descrição 1001, preço 0 e -1, categoria fora do enum; válido passa),
    CA-08 (vazio, 101; 100 passa)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*ValidatorsTests"` → verde

- [x] **T04. `OrderService` valida, apara e lança as exceções novas em português**
  - Arquivos: `Application/Services/OrderService.cs`, `Application/Services/Interfaces/IOrderService.cs`
    (`GetComandaAsync` valida a query), `UnitTests/Application/OrderServiceTests.cs` (ajuste dos tipos + casos novos)
  - Cobre: CA-02, CA-03, CA-05, CA-09 (inativo → `BusinessRuleException` "O prato X não está disponível.";
    mesa/prato inexistente → `NotFoundException`), nada gravado nem publicado na falha; 001/CA-04 e 05 ajustados
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.OrderServiceTests"` → verde

- [x] **T05. `DishService`, `TableService` e `AuthService`**
  - Arquivos: `Application/Services/{DishService,TableService,AuthService}.cs`, `Interfaces/IAuthService.cs`
    (`Login` → `TokenResponseDto?`), `UnitTests/Application/{DishServiceTests,TableServiceTests,AuthServiceTests}.cs`
  - Cobre: CA-07, CA-08 (recusa sem chamar `AddAsync`/`UpdateAsync`; nome gravado aparado), 001/CA-10 (credencial
    errada → `null`)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests` → verde

- [x] **T06. Limites nas configurações do EF**
  - Arquivos: `Infrastructure/Data/Configurations/{Order,OrderItem,ComandaClosure,Dish,Table}Configuration.cs`
  - Cobre: RN-02 a RN-04 (banco e validação com o mesmo número)
  - Verificação: os testes funcionais sobem a API com `Migrate()`, que no EF Core 10 falha se o modelo divergir das
    migrations (`PendingModelChangesWarning`). O `dotnet-ef` não está instalado neste ambiente.

- [x] **T07. Handler global, ProblemDetails e controllers sem `try/catch`**
  - Arquivos: `API/Errors/{ProblemTypes,GlobalExceptionHandler,InvalidModelStateFactory}.cs`, `API/Program.cs`,
    `API/Controllers/{Order,Dish,Table,Auth}Controller.cs`,
    `FunctionalTests/Infrastructure/{HamgueriaApiFactory,ApiClient}.cs` (rota `/__test/boom`, `ReadProblemAsync`),
    `FunctionalTests/ErrorFormatTests.cs`
  - Cobre: CA-10 (500 com title neutro e `traceId`, sem "SEGREDO" nem stack), RN-07 (401 sem login, 403 com token sem
    papel se aplicável, 404 de `GetById` vazio, JSON malformado e enum desconhecido → 400 validacao em português),
    001/CA-10 (401 em ProblemDetails)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.ErrorFormatTests"` → verde

- [x] **T08. Endpoints de pedido e comanda no formato novo**
  - Arquivos: `FunctionalTests/OrderEndpointsTests.cs`, `FunctionalTests/ComandaEndpointsTests.cs`
  - Cobre: CA-01 a CA-06 e CA-09 pela API (POST e PUT de pedido, `comanda/close`, `GET comanda`), conferindo `type`,
    chaves de `errors` e mensagens; 001/CA-04, 05 e 08 ajustados
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*EndpointsTests"` → verde

- [x] **T09. Endpoints de prato e mesa (Admin)**
  - Arquivos: `FunctionalTests/DishEndpointsTests.cs`, `FunctionalTests/TableEndpointsTests.cs`
  - Cobre: CA-07, CA-08 (recusa por campo e `GET` inalterado depois), sucesso de criar/alterar, 401 sem login
  - Verificação: `dotnet test --solution backend/SoftwareHamgueria.slnx` → verde (suíte completa do servidor)

## Fase 2: interface
- [x] **T10. Leitura do ProblemDetails no `shared/api`**
  - Arquivos: `frontend/src/shared/api/problem.ts`, `frontend/src/shared/api/problem.test.ts`, export no índice do
    `shared/api` (se houver; senão import direto do arquivo, como o `httpClient`)
  - Cobre: RN-06, RN-07 (validacao com `errors`, regra com `detail`, 500 com title neutro, erro de rede → fallback)
  - Verificação: `cd frontend && npm test -- problem` → verde

- [x] **T11. Handlers MSW validam com as mesmas mensagens**
  - Arquivos: `frontend/src/shared/mocks/validation.ts`, `validation.test.ts`, `handlers.ts` (pedido POST/PUT,
    `comanda/close`, `GET comanda`, 401/404 com corpo, `POST/PUT /api/dish` e `/api/table` Admin), `data.ts`
    (prato inativo, se faltar)
  - Cobre: CA-13 (tabela de casos de CA-02 a CA-09 com as mensagens dos validadores do servidor)
  - Verificação: `cd frontend && npm test -- validation` → verde

- [x] **T12. Envio do pedido mostra o erro junto do campo**
  - Arquivos: `features/submit-order/model/useSubmitOrder.ts` (+ teste ampliado),
    `features/submit-order/ui/CheckoutDialog.tsx`, `features/submit-order/ui/CheckoutDialog.test.tsx`
  - Cobre: CA-11 (nome inválido → mensagem abaixo do input com `aria-invalid`/`aria-describedby`; erros de item com o
    nome do prato num bloco `role="alert"`; carrinho intacto), CA-12 (prato inativo → toast com a mensagem do sistema)
  - Verificação: `cd frontend && npm test -- submit-order` → verde

- [x] **T13. Limite de 20 no stepper**
  - Arquivos: `shared/ui/molecules/QuantityStepper.tsx` (prop `max`), `widgets/order-json/ui/OrderJsonPanel.tsx`,
    `widgets/order-json/ui/OrderJsonPanel.test.tsx`
  - Cobre: CA-03 na tela (botão "Aumentar" desabilitado em 20)
  - Verificação: `cd frontend && npm test -- OrderJsonPanel` → verde

- [x] **T14. Fechar comanda e ações da cozinha com a mensagem do sistema**
  - Arquivos: `features/close-comanda/model/useCloseComanda.ts` (+ `useCloseComanda.test.tsx`),
    `features/manage-order/model/useManageOrder.ts` (+ `useManageOrder.test.tsx`)
  - Cobre: CA-12 (fechar comanda recusada), CA-14 (finalizar/cancelar com 404 → toast com a mensagem do sistema)
  - Verificação: `cd frontend && npm test -- close-comanda manage-order` → verde

- [x] **T15. Ponta a ponta e gates de interface**
  - Arquivos: `frontend/e2e/cliente.spec.ts` (+ teste: carrinho para em 20 e o pedido no limite é aceito),
    `frontend/e2e/support.ts` (`addFirstDish`, `sendCart`), `docs/audits/2026-10-31-submit-order.md`
  - Cobre: CA-03 e CA-01 no navegador (mock). O passo "nome com 61 caracteres" não cabe no e2e: a tela já impede
    (`maxLength` 60, botão desabilitado com nome vazio), então CA-11 fica nos testes de tela com o handler trocado.
  - Verificação: `cd frontend && npm test && npm run build && npm run lint && npm run e2e` → verde;
    `killer-scan`, `copy-lint` e `motion-lint` em `frontend/src/features/submit-order` e `frontend/src/shared/ui/molecules`
    sem achado novo (relatório em `docs/audits/`)

## Backlog
<!-- Achados da simplify e da revisão que ficam fora do escopo desta spec. Um item por linha, com o motivo. -->
- `System.Security.Cryptography.Pkcs`/`Xml` 9.0.0 com vulnerabilidades conhecidas (NU1902/NU1903) vindas por
  dependência transitiva da API e da Infrastructure. Já existia antes da 003.
- Gate Visant de `CheckoutDialog`/`OrderJsonPanel` fechado por achados anteriores à 003 (hex dos semáforos, emoji
  `➕`, `code-crumb`), já em "Candidatos a spec". Detalhe em `docs/audits/2026-10-31-submit-order.md`.
- `ComandaClosure` aceita fechar comanda de mesa inexistente (o fechamento não confere a mesa). Fora do escopo da 003.
- Avisos `xUnit1051` (CancellationToken) nos testes funcionais da 002. Só ruído de build.
