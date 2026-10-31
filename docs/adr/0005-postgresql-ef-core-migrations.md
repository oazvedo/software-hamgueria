# 0005. PostgreSQL + EF Core, migrations e seed na inicialização

- **Status:** Aceito
- **Data:** 2026-10-31 (retroativo)

## Contexto
O projeto precisa de um banco relacional local fácil de subir e de evoluir o schema sem apagar dados à mão
(o histórico do `Program.cs` mostra que antes o banco era recriado manualmente).

## Decisão
- **PostgreSQL 16** via `backend/docker-compose.yml` e **EF Core + Npgsql**. Connection string `ConnectionStrings:Postgres`.
- **Uma `XxxConfiguration : IEntityTypeConfiguration<Xxx>` por entidade** em `Infrastructure/Data/Configurations/`,
  aplicada explicitamente no `AppDbContext.OnModelCreating`. Tamanhos máximos e índices ficam na configuração.
- **Migrations** em `Infrastructure/Migrations/`, geradas com `dotnet ef migrations add` (o `AppDbContextFactory`
  permite rodar sem subir a API).
- **Na inicialização** a API roda `db.Database.Migrate()` e depois `DatabaseSeeder.Seed(db)` (pratos + 8 mesas, só se
  o banco estiver vazio). Não rode `dotnet ef database update` à mão.

## Alternativas consideradas
- **`EnsureCreated`:** não evolui o schema; era o motivo dos wipes manuais.
- **Aplicar migrations só por comando/CI:** mais seguro em produção, mas o projeto é local/portfólio e o objetivo é subir com um comando.

## Consequências
- ✅ `dotnet run` deixa o banco pronto, com dados de exemplo.
- ⚠️ Uma migration destrutiva é aplicada sem confirmação; revise toda migration que remova coluna ou tabela.
- ⚠️ A connection string está duplicada no `AppDbContextFactory` e no `appsettings.json`.
