# 0008. API REST com Controllers

- **Status:** Proposto
- **Data:** 2026-10-31 (retroativo)

## Contexto
A API serve o cardápio, as mesas, os pedidos e as comandas para o frontend e para a tela da cozinha. Os controllers
atuais seguem um padrão comum que precisa ser registrado para as próximas features.

## Decisão
- Controllers com `[ApiController]` e `[Route("api/[controller]")]`, ou seja, **rota no singular** (`/api/dish`,
  `/api/order`, `/api/table`). Construtor primário injetando só o `IXxxService`. **Nenhuma regra de negócio no controller.**
- Verbos: `GET` (lista e `{id:guid}`), `POST` → `201 CreatedAtAction(nameof(GetById))`, `PUT {id}` → 200 com o DTO,
  e ações de estado como `PATCH {id}/<acao>` → 204 (`enable`, `disable`, `cancel`, `status`).
- **Sem `DELETE`:** entidades são desativadas (`IsActive`) ou mudam de status. Exceção só justificada no `plan.md`.
- **Erros (estado atual):** o service lança `KeyNotFoundException` (→ 404) e `InvalidOperationException` (→ 400),
  e o controller converte com `try/catch`. `GetById` devolve `null` → 404. Sem ProblemDetails nem validação declarativa.
- Documentação com **Swagger** em `/swagger` (só em Development), com esquema Bearer.

## Alternativas consideradas
- **Minimal APIs:** menos cerimônia, mas o requisito pediu Controllers e o código inteiro já usa.
- **Rotas no plural (`/api/dishes`):** mais idiomático, mas quebraria o frontend e os mocks sem ganho de comportamento.

## Consequências
- ✅ Padrão previsível; uma ação nova segue o formato das existentes.
- ⚠️ O tratamento de erro se repete em cada ação e as respostas de erro são texto simples. Candidato a spec:
  validação (FluentValidation) + ProblemDetails + handler global de exceções, via ADR novo.
