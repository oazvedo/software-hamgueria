# 003. Validação de entrada e erros padronizados: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0005, 0006, 0007, 0008 (exceto a regra de erros), 0009, 0011, 0012, 0013, 0014, 0015, 0016
- **ADRs novos propostos:** [0021](../../adr/0021-validacao-de-entrada-com-fluentvalidation.md) (FluentValidation na
  Application), [0022](../../adr/0022-erros-em-problemdetails-com-handler-global.md) (ProblemDetails + handler global;
  substitui a regra de erros do 0008)

## Abordagem
Cada DTO de entrada ganha um validador FluentValidation na Application, chamado pelo service antes de tocar no
repositório (ADR 0021). Os services passam a lançar `NotFoundException` e `BusinessRuleException` próprias, com
mensagem em português, e um `IExceptionHandler` converte tudo em ProblemDetails com `type` por categoria e `traceId`
(ADR 0022). Os controllers perdem todos os `try/catch` e os `if` de validação. No frontend, um módulo em `shared/api`
lê o ProblemDetails do erro do axios; o envio do pedido mostra os erros por campo no `CheckoutDialog` e as recusas sem
campo no toast, e os handlers MSW passam a validar com as mesmas mensagens. Os status HTTP de hoje não mudam.

## Modelo de dados
Sem mudança de schema. Sem migration. As `XxxConfiguration` trocam os números do `HasMaxLength` pelas constantes de
`Limits` (mesmos valores: comanda 60, prato 200/1000, mesa 100, observação 200), então o modelo do EF não muda;
conferir com `dotnet ef migrations add Check` vazio e descartar.

## Contrato da API
Rotas, verbos e sucessos iguais aos de hoje. Muda o **corpo de erro** de todas as rotas: `application/problem+json`.

```json
{
  "type": "https://hamgueria.dev/erros/validacao",
  "title": "Alguns dados do pedido não passaram na checagem.",
  "status": 400,
  "traceId": "00-4bf9...-01",
  "errors": {
    "customerName": ["Informe o nome da comanda."],
    "items[1].quantity": ["A quantidade do item 2 vai de 1 a 20."]
  }
}
```

| Verbo | Rota | Acesso | Validador | Erros |
|---|---|---|---|---|
| POST | `/api/order` | Público | `CreateOrderDtoValidator` | 400 validacao, 400 regra (prato inativo), 404 (mesa/prato) |
| PUT | `/api/order/{id}` | Público | `UpdateOrderDtoValidator` | 400 validacao, 400 regra, 404 |
| POST | `/api/order/comanda/close` | Público | `CloseComandaDtoValidator` | 400 validacao |
| GET | `/api/order/comanda?tableId&customer` | Público | `ComandaQueryValidator` (a checagem sai do controller e vai para o service) | 400 validacao |
| POST / PUT | `/api/dish`, `/api/dish/{id}` | Admin | `CreateDishDtoValidator`, `UpdateDishDtoValidator` | 400 validacao, 401, 403, 404 |
| POST / PUT | `/api/table`, `/api/table/{id}` | Admin | `CreateTableDtoValidator`, `UpdateTableDtoValidator` | 400 validacao, 401, 403, 404 |
| PATCH | `/api/order/{id}/cancel`, `/status`, `/api/dish|table/{id}/(en|dis)able` | conforme hoje | nenhum | 401, 403, 404 |
| POST | `/api/auth/login` | Público | nenhum | 401 sem-login (`AuthService.Login` devolve `null`) |
| qualquer | qualquer | | | 500 falha (title neutro + `traceId`, sem `detail`) |

Regras dos validadores (RN-01 a RN-05): comanda aparada 1–60; itens ≥ 1; `dishId` ≠ vazio; quantidade 1–20 por item
**e** soma por `dishId` ≤ 20 (erro no último item repetido); observação ≤ 200; mesa `tableId` ≠ vazio; prato: nome
aparado 1–200, descrição ≤ 1000, preço > 0, categoria `IsInEnum`; mesa: nome aparado 1–100.
Mensagem de regra: `"O prato {nome} não está disponível."`; não encontrado: `"A mesa informada não existe."`,
`"O prato informado não existe."`, `"Pedido não encontrado."`.

Eventos RabbitMQ: sem mudança. Validação falha antes de gravar, então nada é publicado (já coberto pelo
`AssertNothingSavedOrPublished` da 002).

## Backend: arquivos
- `Application/SoftwareHamgueria.Application.csproj`: + `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`
  (versão exata).
- `Application/Exceptions/NotFoundException.cs`, `BusinessRuleException.cs` (novos).
- `Application/Validators/Limits.cs` (novo) e `Application/Validators/{Order,Dish,Table}/*Validator.cs`:
  `CreateOrderDtoValidator`, `OrderItemDtoValidator`, `UpdateOrderDtoValidator`, `CloseComandaDtoValidator`,
  `ComandaQueryValidator` (record `ComandaQuery(Guid TableId, string Customer)`), `CreateDishDtoValidator`,
  `UpdateDishDtoValidator`, `CreateTableDtoValidator`, `UpdateTableDtoValidator`.
- `Application/ApplicationModule.cs`: `AddValidatorsFromAssemblyContaining<...>()`.
- `Application/Services/OrderService.cs`, `DishService.cs`, `TableService.cs`: injetam `IValidator<T>`, validam
  primeiro, aparam nomes, trocam as exceções e as mensagens para português. `GetComandaAsync` recebe a validação da
  query que hoje está no controller.
- `Application/Services/AuthService.cs` + `IAuthService`: `Login` devolve `TokenResponseDto?` (`null` = negado).
- `Infrastructure/Data/Configurations/{Order,OrderItem,ComandaClosure,Dish,Table}Configuration.cs`: `Limits.*`.
- `API/Errors/ProblemTypes.cs` (constantes de `type` e `title`), `API/Errors/GlobalExceptionHandler.cs`
  (`IExceptionHandler` + `ILogger`), `API/Errors/InvalidModelStateFactory.cs` (erro de leitura do corpo).
- `API/Program.cs`: `AddProblemDetails` (com `CustomizeProblemDetails`: `traceId` e títulos em português por status),
  `AddExceptionHandler<GlobalExceptionHandler>`, `ConfigureApiBehaviorOptions`,
  `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes`, `UseExceptionHandler()`, `UseStatusCodePages()`.
- `API/Controllers/*.cs`: sem `try/catch`, sem `BadRequest("...")`; `AuthController` vira `null → Unauthorized()`.
- Testes: `UnitTests/Application/Validators/*ValidatorTests.cs` (novos), `UnitTests/Application/OrderServiceTests.cs`,
  `DishServiceTests.cs`, `TableServiceTests.cs` (novos para os services de prato e mesa),
  `FunctionalTests/ErrorFormatTests.cs` (novo: formato, 401/403/404/500), `FunctionalTests/OrderEndpointsTests.cs`,
  `ComandaEndpointsTests.cs`, `AuthEndpointsTests.cs`, `DishEndpointsTests.cs`, `TableEndpointsTests.cs` (novos),
  `FunctionalTests/Infrastructure/ApiClient.cs` (helper `ReadProblemAsync`). Para o 500, um endpoint de teste
  registrado só na factory (`/__test/boom`) que lança exceção com texto "SEGREDO" e o teste confere que não vaza.

## Frontend: arquivos
- `shared/api/problem.ts` (novo) + export em `shared/api`: tipo `ApiProblem`, `readProblem(error): ApiProblem | null`
  (axios com corpo `problem+json`), `problemMessage(error, fallback)`, `fieldErrors(problem)`. Teste
  `problem.test.ts`.
- `shared/mocks/validation.ts` (novo): mesmas regras e mensagens do servidor, e `problem(...)`/`validationProblem(...)`
  que montam o corpo. `shared/mocks/handlers.ts`: `POST /api/order` valida (400 validacao), checa mesa/prato (404) e
  prato inativo (400 regra); `PUT /api/order/:id` idem; `comanda/close` e `GET comanda` com 400 validacao; 401/404
  com corpo ProblemDetails; handlers novos `POST/PUT /api/dish`, `POST/PUT /api/table` (Admin, validam e devolvem o
  DTO, sem persistir). `shared/mocks/data.ts`: um prato inativo para o cenário de regra, se não houver.
- `features/submit-order/model/useSubmitOrder.ts`: `onError` mostra `problemMessage` no toast só quando não há erro
  por campo; expõe o problema da mutação para a tela. Carrinho intacto no erro (já é assim; vira teste).
- `features/submit-order/ui/CheckoutDialog.tsx`: mensagem de `customerName` abaixo do input (`aria-invalid`,
  `aria-describedby`); erros de item listados no diálogo com o nome do prato (`items[i]` → `items[i].name`), num bloco
  com `role="alert"`. Limpa o erro ao editar o nome.
- `widgets/order-json/ui/OrderJsonPanel.tsx` + `shared/ui/molecules/QuantityStepper.tsx`: `max` 20 no stepper
  (botão "Aumentar" desabilitado no limite). Prevenção no cliente; o servidor continua sendo a regra.
- `features/close-comanda/model/useCloseComanda.ts`, `features/manage-order/model/useManageOrder.ts`: `onError` com
  `problemMessage(error, '<texto atual>')` (CA-12, CA-14).
- Chaves de query: sem mudança (erro não invalida nada).
- Testes: `useSubmitOrder.test.tsx` (ampliado), `CheckoutDialog.test.tsx` (novo), `useCloseComanda.test.tsx`,
  `useManageOrder.test.tsx` (novos), `shared/mocks/validation.test.ts` (novo: mesmas mensagens do servidor para a
  mesma entrada), `e2e/cliente.spec.ts` (+ passo: nome com 61 caracteres colado → mensagem abaixo do campo).

Superfície (`visant-frontend`): **B, trabalho** (checkout do cliente). Cut test: a mensagem por campo move a taxa de
pedido enviado depois de uma recusa; o bloco de erros de item só aparece quando há erro. Copy nova passa pelo
`copy-lint` (sem travessão, emoji ou ponto-e-vírgula); tom sério e direto nos erros (`better-writing`).

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | Unidade + funcional + e2e | `CreateOrderDtoValidatorTests` (válido, 20 itens, observação com 200); teste 001/CA-03 existente verde; `npm run e2e` |
| CA-02 | Unidade + funcional | `"003/CA-02: pedido sem itens é recusado"`: `errors.items`, nada gravado nem publicado |
| CA-03 | Unidade + funcional | Theory 0, -1, 21 e soma 15+6 do mesmo prato; POST e PUT; `errors["items[0].quantity"]` |
| CA-04 | Unidade + funcional | 201 caracteres recusa `items[0].notes` com "200"; 200 aceita |
| CA-05 | Unidade + funcional | vazio, "   ", 61 recusa; 60 aceita; POST pedido e `comanda/close` |
| CA-06 | Unidade + funcional | sem nome + quantidade 0 → `errors` com as duas chaves numa resposta |
| CA-07 | Unidade + funcional | `DishEndpointsTests` com admin: nome vazio, preço 0 e -1, 201/1001 caracteres, categoria inválida; `GET /api/dish` inalterado |
| CA-08 | Unidade + funcional | `TableEndpointsTests`: vazio, 101; `GET /api/table` inalterado |
| CA-09 | Unidade + funcional | prato inativo → 400 `type` regra, `detail` "O prato X não está disponível."; mesa/prato inexistente → 404 em português |
| CA-10 | Funcional | `/__test/boom` → 500, title neutro, `traceId`, corpo sem "SEGREDO", sem `stackTrace`/`exception` |
| CA-11 | Vitest + e2e | `CheckoutDialog.test.tsx`: handler devolve erro de `customerName` → texto abaixo do input, itens do carrinho intactos |
| CA-12 | Vitest | `useSubmitOrder` com prato inativo → toast com a mensagem do sistema; `useCloseComanda` com 400 → idem |
| CA-13 | Vitest | `validation.test.ts`: tabela de entradas de CA-02 a CA-09 com mensagens iguais às dos validadores (lista copiada dos testes do servidor); testes de tela rodam contra os handlers |
| CA-14 | Vitest | `useManageOrder.test.tsx`: finalizar/cancelar com 404 → toast com a mensagem do sistema |
| 001/CA-04, 05, 10 | Funcional (ajuste) | Testes da 002 passam a conferir `type`/`title` e mensagem em português |

## Riscos e decisões pendentes
- **ADRs 0021 e 0022 aceitos** (2026-10-31). Falta aplicar nas referências: status do 0008 vira
  "Substituído em parte por 0022" e a skill `dotnet-feature` (passos 7 e 9) e o `CLAUDE.md` (Regras rápidas) citam o
  novo padrão. Isso é a T01.
- **Divergência mock × servidor (ADR 0013):** as mensagens ficam escritas duas vezes (C# e TS). Mitigação: o
  `validation.test.ts` usa a mesma tabela de casos e mensagens dos testes do servidor; e o `e2e:real` pega o resto.
- **Mensagem do serializador:** enum desconhecido ou JSON malformado não passa pelo FluentValidation; o
  `InvalidModelStateFactory` troca a mensagem por "Valor inválido." no campo. Testado em `ErrorFormatTests`.
- **`UseStatusCodePages` e 401 do JWT:** o desafio do JwtBearer escreve o 401 sem corpo; confirmar que o
  `UseStatusCodePages` preenche o ProblemDetails (se não, `OnChallenge` no `AddJwtBearer`).
- **Soma por prato:** o erro aparece no último item repetido (`items[i].quantity`). Na tela isso nunca acontece,
  porque o carrinho já agrupa por prato; é proteção para chamadas diretas à API.
- **`traceId` no 500:** é o `Activity.Current?.Id ?? HttpContext.TraceIdentifier`; o log estruturado do handler grava
  o mesmo valor. Sem ferramenta de busca de log nesta spec: o mantenedor procura no console da API.
- **Fora desta spec (vai para o Backlog se aparecer):** mensagens do frontend que não vêm do servidor, copy antiga,
  `transition-all` do `button.tsx`.
