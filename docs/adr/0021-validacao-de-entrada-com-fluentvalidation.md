# 0021. Validação de entrada com FluentValidation na Application

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
A API não valida a entrada (spec 003). Só o nome da comanda é checado, com `if` solto no `OrderService.CreateAsync`,
no `CloseComandaAsync` e de novo no `OrderController`. Pedido sem itens, quantidade 0, preço negativo e nome de prato
vazio passam, e textos longos só falham no `SaveChanges` (limites do `HasMaxLength`). O ADR 0008 já apontava
FluentValidation como caminho. A regra precisa ser testável na unidade (ADR 0016) e trazer todas as falhas de uma vez,
em português.

## Decisão
- Usamos **FluentValidation** (pacotes `FluentValidation` e `FluentValidation.DependencyInjectionExtensions`, versão
  exata) **no projeto Application**. Um validador por DTO de entrada, em `Application/Validators/<Entidade>/`,
  nomeado `<Dto>Validator`.
- O **service** valida antes de qualquer leitura de repositório, com `await validator.ValidateAndThrowAsync(dto)`.
  O controller não valida nada. Regras que dependem do banco (prato inativo, mesa inexistente) continuam no service.
- Mensagens em português, com o nome do campo para o cliente (`WithName("nome da comanda")`) e o limite quando houver.
  Os limites ficam numa classe de constantes (`Application/Validators/Limits.cs`) que as `XxxConfiguration` também
  usam no `HasMaxLength`, para banco e validação não divergirem.
- Texto aparado: o validador mede o texto sem os espaços das pontas, e o service grava aparado.
- Registro com `services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>()` no `ApplicationModule.cs`.
- No `Program.cs`, `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`: o `[Required]` implícito
  do MVC não roda antes do validador (mensagem em inglês e duplicada).

```csharp
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).Must(n => !string.IsNullOrWhiteSpace(n))
            .WithName("nome da comanda").WithMessage("Informe o nome da comanda.");
        RuleFor(x => x.Items).NotEmpty().WithName("itens").WithMessage("O pedido precisa de pelo menos um item.");
        RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
    }
}
```

## Alternativas consideradas
- **DataAnnotations nos DTOs `record`:** atributos em parâmetro posicional exigem `[property: ...]`, regras de coleção
  (soma por prato, item N) não cabem, e as mensagens em português ficam espalhadas pelos atributos.
- **Validação automática no pipeline do MVC (filtro ou `FluentValidation.AspNetCore`):** o pacote de auto-validação
  foi descontinuado pelo próprio projeto, e a regra sairia do alcance dos testes de unidade do service.
- **Validar no Domain (construtor da entidade):** é o lugar ideal para invariantes, mas para na primeira falha (não
  junta todos os problemas, RN-06) e exigiria mensagens de interface no Domain.

## Consequências
- ✅ Regras de entrada declaradas num lugar só, testáveis sem banco, com todas as falhas de uma vez.
- ✅ Limites do banco e da validação vêm da mesma constante.
- ⚠️ Mais uma dependência na Application. Atualizar a versão é decisão consciente.
- ⚠️ As invariantes não moram na entidade: quem criar `Order` fora do service não é validado. Aceito enquanto o
  service for a única porta de escrita.
- ⚠️ As mesmas regras são reescritas nos handlers MSW (ADR 0013); a divergência é coberta pelos testes de tela.
