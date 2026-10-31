# 0007. DTOs `record` com mapeamento manual

- **Status:** Proposto
- **Data:** 2026-10-31 (retroativo)

## Contexto
A API não deve expor as entidades do Domain (navegações, setters privados, campos internos). Os DTOs atuais já são
`record` posicionais e o mapeamento é feito à mão nos services.

## Decisão
- DTOs em `Application/DTOs/<Entidade>/`: `CreateXxxDto`, `UpdateXxxDto`, `XxxResponseDto` (e DTOs de ação, como
  `CloseComandaDto`), todos `record` posicionais.
- O mapeamento entidade → DTO é um método `private static ToDto(...)` no próprio service. Sem AutoMapper/Mapster.
- Enums trafegam como **string** no JSON (`JsonStringEnumConverter` no `Program.cs`), e o frontend espelha os nomes
  (`'Created' | 'Draft' | 'Cancelled' | 'Done'`).

```csharp
public record CreateDishDto(string Name, string Description, double Price, Category Category);
private static DishResponseDto ToDto(Dish d) =>
    new(d.Id, d.Name, d.Description, d.Price, d.Category, d.IsActive, d.CreatedAt, d.UpdatedAt);
```

## Alternativas consideradas
- **AutoMapper/Mapster:** esconde o mapeamento e os erros só aparecem em runtime.
- **Expor as entidades:** vazaria navegações e acoplaria o contrato ao schema.

## Consequências
- ✅ Mapeamento explícito e fácil de seguir.
- ⚠️ Os tipos do frontend (`entities/*/model/types.ts`) são mantidos à mão e podem divergir; não há geração de cliente.
