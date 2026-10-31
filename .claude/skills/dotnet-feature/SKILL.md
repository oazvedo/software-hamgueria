---
name: dotnet-feature
description: Receita para criar ou alterar uma entidade, endpoint, service, repositório, mensagem RabbitMQ ou migration no backend .NET do Software Hamgueria (DDD em Domain/Application/Infrastructure/API, repositórios que persistem a própria escrita, DTOs record, controllers em /api/<entidade>, JWT Admin). Use sempre que for mexer em backend/.
---

# Skill: dotnet-feature

Regras completas nos ADRs 0004 a 0010, 0014, 0021 (validação) e 0022 (erros). Esta skill é a **receita passo a passo**, escrita a partir do código
que já existe. Os exemplos usam `Table`, o recurso mais simples; para outra entidade, troque o nome
(código em inglês, ADR 0004). **Copie o padrão existente; não "melhore" a base sem um ADR.**

## Ordem dos passos (cada passo compila antes do próximo)

### 1. Domain: entidade (`SoftwareHamgueria.Domain/Entities/Table.cs`)
```csharp
namespace SoftwareHamgueria.Domain.Entities;

public class Table
{
    public Guid Id { get; private set; }
    public string TableName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    protected Table() { }                    // EF Core

    public Table(string tableName)
    {
        Id = Guid.NewGuid();
        TableName = tableName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string tableName) { TableName = tableName; UpdatedAt = DateTime.UtcNow; }
    public void Disable() => IsActive = false;
    public void Enable() => IsActive = true;
}
```
Setters privados, mudanças por métodos com nome, datas em UTC, nada de atributos de EF.
Enums novos vão em `Domain/Enums/` com namespace `SoftwareHamgueria.Domain.Entities` (como os atuais).

### 2. Application: porta do repositório (`Application/Repositories/ITableRepository.cs`)
```csharp
public interface ITableRepository
{
    Task AddAsync(Table table);
    Task UpdateAsync(Table table);
    Task<Table?> GetByIdAsync(Guid id);
    Task<IEnumerable<Table>> GetAllAsync();
}
```
Consultas específicas (ex.: `GetByTableAsync`) entram aqui com nome que descreve o filtro.

### 3. Infrastructure: configuração (`Infrastructure/Data/Configurations/TableConfiguration.cs`)
```csharp
public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TableName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.IsActive).HasDefaultValue(true);
    }
}
```
Registre no `AppDbContext`: um `DbSet<Table> Tables => Set<Table>();` e
`modelBuilder.ApplyConfiguration(new TableConfiguration());`. FKs com `OnDelete(DeleteBehavior.Restrict)`,
exceto filhos de agregado (ex.: `OrderItem`), que usam `Cascade`.

### 4. Infrastructure: repositório (`Infrastructure/Repositories/TableRepository.cs`)
```csharp
public class TableRepository(AppDbContext context) : ITableRepository
{
    public async Task AddAsync(Table table)
    {
        await context.Tables.AddAsync(table);
        await context.SaveChangesAsync();          // ADR 0006: o repositório persiste a própria escrita
    }

    public async Task UpdateAsync(Table table)
    {
        context.Tables.Update(table);
        await context.SaveChangesAsync();
    }

    public Task<Table?> GetByIdAsync(Guid id) => context.Tables.FirstOrDefaultAsync(t => t.Id == id);
    public async Task<IEnumerable<Table>> GetAllAsync() => await context.Tables.ToListAsync();
}
```
Se o DTO precisa de navegação, faça `Include` aqui (veja `OrderRepository`). Registre no `InfrastructureModule.cs`:
`services.AddScoped<ITableRepository, TableRepository>();`

### 5. Migration
```bash
dotnet ef migrations add AddTable \
  -p backend/src/SoftwareHamgueria.Infrastructure -s backend/src/SoftwareHamgueria.API
```
Revise o arquivo gerado (tabela, colunas, tamanhos, índices, FKs). **Não** rode `database update`: a migration é
aplicada no próximo `dotnet run` (ADR 0005). Se a entidade precisa de dados iniciais, acrescente no `DatabaseSeeder`
sem quebrar a guarda `if (context.Dishes.Any()) return;`.

### 6. Application: DTOs (`Application/DTOs/Table/`)
```csharp
public record CreateTableDto(string TableName);
public record UpdateTableDto(string TableName);
public record TableResponseDto(Guid Id, string TableName, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
```
Um arquivo por DTO (`CreateTableDto.cs`, ...). Enums vão como o tipo do Domain; viram string no JSON.

### 7. Application: service
`Application/Services/Interfaces/ITableService.cs` + `Application/Services/TableService.cs`:
```csharp
public class TableService(ITableRepository repository, IValidator<UpdateTableDto> updateValidator) : ITableService
{
    public async Task<TableResponseDto> UpdateAsync(Guid id, UpdateTableDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);          // ADR 0021: valida antes de tocar no repositório
        var table = await repository.GetByIdAsync(id) ?? throw new NotFoundException("Mesa não encontrada.");
        table.Update(dto.TableName.Trim());
        await repository.UpdateAsync(table);
        return ToDto(table);
    }

    public async Task<TableResponseDto?> GetByIdAsync(Guid id)
    {
        var table = await repository.GetByIdAsync(id);
        return table is null ? null : ToDto(table);
    }

    private static TableResponseDto ToDto(Table t) => new(t.Id, t.TableName, t.IsActive, t.CreatedAt, t.UpdatedAt);
}
```
Regras: validar → carregar → regra de negócio → método da entidade → **uma** escrita de repositório → (publicar evento) → DTO.
- Entrada inválida: validador FluentValidation em `Application/Validators/<Entidade>/<Dto>Validator.cs`, mensagens em
  português com `WithName("<campo para o cliente>")`, limites de `Validators/Limits.cs` (ADR 0021).
- Não encontrado: `NotFoundException`. Regra do restaurante violada: `BusinessRuleException`. Ambas de
  `Application/Exceptions/`, com mensagem em português para o cliente. Nunca `KeyNotFoundException` nem
  `InvalidOperationException` (o framework lança os mesmos tipos, ADR 0022).
- Se a operação precisa gravar dois agregados juntos, **pare**: isso pede o ADR de Unit of Work (ADR 0006).
- Registre no `ApplicationModule.cs`: `services.AddScoped<ITableService, TableService>();`

### 8. Eventos (só se a spec pedir)
Injete `IOrderEventPublisher` e publique **antes** da escrita do repositório (outbox, ADR 0031): `publisher.Publish("order.created", order.Id.ToString())`.
`Publish` grava no outbox (sem salvar) e deve vir **antes** da escrita do repositório, que salva os dois juntos; o envio é em segundo plano (ADR 0031).
Routing key `<agregado>.<evento-no-passado>` no exchange `hamgueria.orders` (ADR 0010). Outro agregado com eventos
próprios = nova porta em `Application/Messaging/` + implementação em `Infrastructure/Messaging/`, decidido no `plan.md`.

### 9. API: controller (`API/Controllers/TableController.cs`)
```csharp
[ApiController]
[Route("api/[controller]")]
public class TableController(ITableService tableService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await tableService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var table = await tableService.GetByIdAsync(id);
        return table is null ? NotFound() : Ok(table);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
    {
        var table = await tableService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPatch("{id:guid}/disable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await tableService.DisableAsync(id);   // sem try/catch: o handler global vira ProblemDetails (ADR 0022)
        return NoContent();
    }
}
```
Nenhuma lógica no controller e nenhum `try/catch`: o `GlobalExceptionHandler` (`API/Errors/`) converte as exceções
em ProblemDetails (ADR 0022). Consulta que devolve `null` vira `NotFound()`. `[Authorize(Roles = "Admin")]` em toda ação que o
`plan.md` marcou como Admin (ADR 0009). Sem `DELETE` (ADR 0008).

### 10. Contrato no frontend
Todo endpoint novo ou alterado precisa, **na mesma spec**, do tipo em `entities/<x>/model/types.ts`, da função em
`entities/<x>/api/` e do handler MSW (skill `react-feature`, ADR 0013).

## Verificação
```bash
dotnet test --solution backend/SoftwareHamgueria.slnx   # testes de unidade (Application) + funcionais (endpoint), com sucesso e falha
```
Cada task inclui o cenário de sucesso e o de falha (unidade em `tests/SoftwareHamgueria.UnitTests`, endpoint em `tests/SoftwareHamgueria.FunctionalTests` usando o `ApiClient`). Para endpoints, valide também pelo
Swagger (`/swagger`) ou pelo `SoftwareHamgueria.API.http`.

## Checklist de pronto
- [ ] Entidade com setters privados, construtor `protected` vazio e métodos com nome
- [ ] Porta `IXxxRepository` na Application; implementação + registro no `InfrastructureModule`
- [ ] `XxxConfiguration` aplicada no `AppDbContext`; migration revisada
- [ ] DTOs `record` + `ToDto` privado no service; service registrado no `ApplicationModule`
- [ ] Validador por DTO de entrada, chamado no service antes do repositório; exceções de `Application/Exceptions/`
- [ ] Uma escrita de repositório por operação; evento publicado depois da escrita (se houver)
- [ ] Controller sem lógica; público/Admin conforme o `plan.md`; sem DELETE
- [ ] Tipo + API + handler MSW no frontend
- [ ] Configuração nova (ADR 0024): chave sensível nunca no `appsettings.json` nem com fallback no código; valor de dev no `appsettings.Development.json`, regra no `RequiredSettings` e variável no `.env.example` + `docker-compose.prod.yml`
- [ ] Testes de unidade e funcional (ADR 0016) verdes; `dotnet run` aplica a migration e o endpoint aparece no `/swagger`
