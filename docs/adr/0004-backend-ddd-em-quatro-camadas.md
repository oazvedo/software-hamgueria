# 0004. Backend em DDD com quatro camadas

- **Status:** Proposto
- **Data:** 2026-10-31 (retroativo)

## Contexto
O `backend/backend-requirements.md` pediu DDD com camadas API, Domain, Application e Infrastructure, DI centralizada
em módulos e o fluxo `Controller → IService → IRepository → Database`. É assim que o código está.

## Decisão
Quatro projetos em `backend/src/`:

| Projeto | Contém | Referencia |
|---|---|---|
| `SoftwareHamgueria.Domain` | `Entities/` e `Enums/` (namespace `SoftwareHamgueria.Domain.Entities`) | nada |
| `SoftwareHamgueria.Application` | `DTOs/<Entidade>/`, `Services/` (+ `Interfaces/`), `Repositories/` (interfaces `IXxxRepository`), `Messaging/` (portas como `IOrderEventPublisher`) e `ApplicationModule.cs` | Domain |
| `SoftwareHamgueria.Infrastructure` | `Data/` (`AppDbContext`, `Configurations/`, `Migrations/`, `DatabaseSeeder`), `Repositories/`, `Messaging/` e `InfrastructureModule.cs` | Application, Domain |
| `SoftwareHamgueria.API` | `Controllers/`, `Filters/` e `Program.cs` | Application, Infrastructure |

- **Interfaces de repositório ficam na Application** (portas), implementadas na Infrastructure (adaptadores).
- **Entidades:** setters privados, construtor `protected` vazio para o EF, construtor público que gera `Id`
  (`Guid.NewGuid()`) e datas UTC, e mudanças por métodos com nome (`Update`, `Disable`, `Cancel`, `AddItem`).
- **DI:** cada camada expõe um módulo, e o `Program.cs` só chama os módulos:
  ```csharp
  builder.Services.AddApplicationServices();
  builder.Services.AddInfrastructureServices(builder.Configuration);
  ```
- **Nomenclatura:** código em inglês (`Dish`, `OrderService`, `CreateAsync`), exceto o termo de domínio `comanda`
  (`IComandaRepository`, `GetComandaAsync`), que não tem tradução fiel. Textos de interface em português.

## Alternativas consideradas
- **Interfaces de repositório no Domain (Clean Architecture clássica):** é o padrão do repositório de estudo, mas
  mudar agora mexe em todos os arquivos sem ganho de comportamento.
- **Vertical slice / projeto único:** perderia as regras de dependência garantidas pelo compilador, que eram um requisito.

## Consequências
- ✅ Cada tipo de código tem um lugar óbvio; a skill `dotnet-feature` vira uma receita.
- ✅ O Domain não depende de EF Core nem de RabbitMQ.
- ⚠️ Nenhum teste de arquitetura garante as regras de dependência ainda (só as referências entre projetos).
- ⚠️ Nomes em inglês diferem do repositório de estudo (que usa português); a regra aqui é manter a consistência com o código existente.
