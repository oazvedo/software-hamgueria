# 0022. Erros da API em ProblemDetails com handler global

- **Status:** Aceito
- **Data:** 2026-10-31
- **Substitui:** a regra de **erros** do [0008](0008-api-rest-com-controllers.md) (o resto do 0008 continua valendo)

## Contexto
Hoje cada ação de controller converte exceção em status com `try/catch` repetido, e o corpo do erro é texto simples em
inglês (`"Dish 'X' is not active."`), vazio (`NotFound()`) ou o padrão do framework. O service lança
`KeyNotFoundException` e `InvalidOperationException`, tipos que o próprio .NET e o EF Core também lançam: um erro
interno do EF viraria 400 com a mensagem interna exposta. A spec 003 pede um formato só, em português, que distinga
entrada inválida, registro inexistente, regra do restaurante, sem permissão e falha inesperada, sem vazar detalhes.

## Decisão
- Toda resposta de erro é **ProblemDetails** (RFC 9457, `application/problem+json`), com `title` em português, `status`,
  `detail` (quando houver), `type` estável e `traceId`. Entrada inválida usa o mesmo formato com `errors`:
  `{ "<caminho do campo em camelCase>": ["mensagem"] }`, ex.: `"items[1].quantity"`.
- `type` por categoria (o status HTTP continua o de hoje):

  | Categoria | Origem | Status | `type` |
  |---|---|---|---|
  | Entrada inválida | `FluentValidation.ValidationException`, erro de leitura do JSON | 400 | `.../validacao` |
  | Regra do restaurante | `BusinessRuleException` | 400 | `.../regra` |
  | Registro inexistente | `NotFoundException` | 404 | `.../nao-encontrado` |
  | Sem login / sem permissão | middleware de autenticação | 401 / 403 | `.../sem-login`, `.../sem-permissao` |
  | Falha inesperada | qualquer outra exceção | 500 | `.../falha` |

  Os `type` são `https://hamgueria.dev/erros/<categoria>` (identificador, não precisa resolver).
- O service lança **exceções próprias** de `Application/Exceptions/` (`NotFoundException`, `BusinessRuleException`),
  com mensagem em português para o cliente. Não lança mais `KeyNotFoundException` nem `InvalidOperationException`.
- Um `IExceptionHandler` em `API/Errors/` (`AddExceptionHandler` + `AddProblemDetails` + `UseExceptionHandler` e
  `UseStatusCodePages` no `Program.cs`) faz o mapeamento. **Controllers sem `try/catch`.** `null` de consulta continua
  virando `NotFound()` no controller, e o corpo vazio vira ProblemDetails pelo `UseStatusCodePages`.
- Falha inesperada: `title` neutro ("Algo deu errado do nosso lado. Tente de novo."), sem `detail`, sem stack, em
  qualquer ambiente. O detalhe vai para o log (`ILogger`) com o mesmo `traceId`.
- Erro de leitura do corpo (JSON malformado, enum desconhecido) usa o `InvalidModelStateResponseFactory` para devolver
  a categoria "entrada inválida", com mensagem em português por campo, sem o texto do serializador.

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
{
    var order = await orderService.CreateAsync(dto);   // sem try/catch: o handler global converte
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}
```

## Alternativas consideradas
- **Manter `try/catch` por ação, trocando só o corpo:** o padrão continuaria se repetindo e cada ação nova poderia
  esquecer um caso.
- **Result pattern (`Result<T>` no service):** elimina exceção como fluxo, mas muda a assinatura de todos os services
  e todos os testes da 002 de uma vez, sem ganho de comportamento para o cliente.
- **Códigos 409/422 para regra do restaurante:** semanticamente melhor, mas quebra o status de 001/CA-05 sem pedido da
  spec. A categoria já fica distinguível pelo `type`.
- **Mapear `KeyNotFoundException`/`InvalidOperationException` no handler:** mais curto, mas expõe mensagens internas
  do framework, que lança os mesmos tipos (fere o RN-08 da spec 003).

## Consequências
- ✅ Um formato só para a tela tratar; controllers mais curtos; nenhuma mensagem interna sai da API.
- ✅ O `traceId` liga o que o cliente viu ao log do servidor.
- ⚠️ Os testes da 002 que esperam `InvalidOperationException`/`KeyNotFoundException` mudam para os tipos novos.
- ⚠️ O 0008 passa a ter uma regra substituída: a skill `dotnet-feature` e o `CLAUDE.md` precisam citar este ADR.
