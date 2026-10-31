# 004. Preparar o Hamgueria para rodar num servidor na nuvem: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, a spec inteira de uma vez.
> **Pré-requisito:** ADRs 0023 e 0024 aceitos (2026-10-31). Docker rodando. O `dotnet test` usa o
> Microsoft.Testing.Platform (`--project` e `--filter-class`).

## Fase 0: decisões
- [x] **T01. Aplicar os ADRs aceitos nas referências**
  - Arquivos: `CLAUDE.md` (Regras rápidas: segredos por ambiente e deploy, ADRs 0023 e 0024),
    `.claude/skills/dotnet-feature/SKILL.md` (configuração nova: chave sensível nunca no `appsettings.json`, entra no
    `RequiredSettings` e no `.env.example`)
  - Cobre: pré-requisito de todas
  - Verificação: `grep -n "0024" CLAUDE.md .claude/skills/dotnet-feature/SKILL.md` → uma linha em cada

## Fase 1: servidor
- [x] **T02. `RequiredSettings` com as regras de produção**
  - Arquivos: `Application/Configuration/RequiredSettings.cs`, `UnitTests/Application/RequiredSettingsTests.cs`
  - Cobre: CA-04 (cada chave ausente é citada pelo nome), CA-05 (`admin123`, senha 11/12, chave 31/32, `guest`),
    mensagens sem os valores
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.RequiredSettingsTests"` → verde

- [x] **T03. `AuthService` sem fallback de senha**
  - Arquivos: `Application/Services/AuthService.cs`, `UnitTests/Application/AuthServiceTests.cs`
  - Cobre: CA-10 (sem usuário ou senha configurados, login negado)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.AuthServiceTests"` → verde
    e `grep -rn '?? "admin' backend/src --include=*.cs` → vazio (o literal `admin123` só existe no `RequiredSettings`,
    para recusá-lo)

- [x] **T04. Segredos fora do `appsettings.json` e validação no startup**
  - Arquivos: `API/appsettings.json`, `API/appsettings.Development.json` (portas 5433/5673),
    `API/Program.cs` (`RequiredSettings.Check` fora de `Development`, antes do `Build()`),
    `Infrastructure/Data/AppDbContextFactory.cs` (ambiente ou dev na 5433),
    `FunctionalTests/ProductionConfigTests.cs`
  - Cobre: CA-04 de ponta (host em `Production` sem `Auth:AdminPassword` não sobe e cita a chave), RN-10
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.ProductionConfigTests"` → verde

- [x] **T05. `GET /api/health` com checagem do banco**
  - Arquivos: `Infrastructure/SoftwareHamgueria.Infrastructure.csproj` (+ `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`
    10.0.0) e `InfrastructureModule.cs` (registro), `API/Program.cs` (só o `MapHealthChecks`),
    `FunctionalTests/HealthEndpointTests.cs`, `FunctionalTests/Infrastructure/HamgueriaApiFactory.cs` (`StopDatabaseAsync`)
  - Desvio do plan: o registro foi para o `InfrastructureModule`, não para o `Program.cs` (DI só nos módulos, ADR 0004).
  - Cobre: CA-09 (200 `Healthy`; banco parado → 503 `Unhealthy` sem detalhe)
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.HealthEndpointTests"` → verde

- [x] **T06. Compose de dev nas portas novas e `backend/.env`**
  - Arquivos: `backend/docker-compose.yml` (`${VAR:-padrão}`), `backend/.env.example`, `backend/.env` (local, ignorado)
  - Cobre: CA-01, CA-02
  - Verificação: `cd backend && docker compose up -d && docker compose ps` mostra 5433, 5673 e 15673; com a API
    rodando, `curl -s localhost:5262/api/dish` devolve o cardápio; `git check-ignore backend/.env` → ignorado

- [x] **T07. Imagem da API**
  - Arquivos: `backend/Dockerfile`, `backend/.dockerignore`
  - Cobre: CA-03 (parte da API)
  - Verificação: `docker build -t hamgueria-api backend` → sucesso; `docker run --rm -e ASPNETCORE_ENVIRONMENT=Production
    hamgueria-api` sai com erro citando as chaves faltantes (CA-04 no container)

## Fase 2: interface
- [x] **T08. Handler MSW de `/api/health`**
  - Arquivos: `shared/mocks/handlers.ts`, teste junto dos handlers
  - Cobre: ADR 0013 para o endpoint novo
  - Verificação: `cd frontend && npm test -- handlers` → verde

- [x] **T09. Imagem da tela com Caddy**
  - Arquivos: `frontend/Dockerfile`, `frontend/.dockerignore`, `frontend/Caddyfile`, `shared/mocks/index.ts` (lê
    `import.meta.env.VITE_ENABLE_MOCKS` direto para o Vite eliminar o chunk do MSW no build de produção)
  - Cobre: CA-08 (build sem mocks), RN-06, RN-07
  - Verificação: `docker build -t hamgueria-web frontend` → sucesso;
    `docker run --rm --entrypoint sh hamgueria-web -c 'grep -l setupWorker /srv/assets/*.js'` → vazio (o MSW ficou fora
    do bundle de produção)

## Fase 3: produção e documentação
- [x] **T10. `docker-compose.prod.yml` e `.env.example` da raiz**
  - Arquivos: `docker-compose.prod.yml`, `.env.example`
  - Cobre: CA-03, CA-06 (só `web` com portas), RN-05
  - Verificação: `docker compose -f docker-compose.prod.yml --env-file .env.example config` falha citando o segredo
    vazio; com um `.env` de teste, `config` passa e só o `web` tem `ports`

- [x] **T11. Script de fumaça da pilha de produção**
  - Arquivos: `scripts/deploy/smoke.sh`
  - Cobre: CA-03, CA-06, CA-07, CA-08, CA-09
  - Verificação: `sh scripts/deploy/smoke.sh` → todos os passos `OK` e pilha derrubada no fim

- [x] **T12. Guia de deploy e documentação**
  - Arquivos: `docs/deploy/aws-ec2.md`, `README.md` (portas novas, link do guia), `CLAUDE.md` (comandos: e2e real
    na 5433, deploy), `docs/specs/README.md` (roadmap e candidato "Segredos fora do `appsettings`" removido)
  - Cobre: CA-03 (passo a passo), CA-10 (`.env` ignorado)
  - Verificação: `grep -rn "5432\|15672" README.md CLAUDE.md` → vazio; `git check-ignore .env` → ignorado;
    suíte completa verde (`dotnet test --solution backend/SoftwareHamgueria.slnx`, `cd frontend && npm test && npm run lint && npm run build`)

## Backlog
<!-- Achados da simplify e da revisão que ficam fora do escopo desta spec. Um item por linha, com o motivo. -->
- Job de CI que roda `docker build` dos dois Dockerfiles (risco do plan: imagem quebrada só aparece no deploy).
- Fixar as imagens base por digest (`aspnet:10.0`, `caddy:2-alpine`, `postgres:16-alpine`).
- `RabbitMqPublisher` conecta no construtor (singleton): depois de `docker compose restart` ou de um reboot da EC2,
  a API pode voltar antes da fila e as rotas de pedido respondem 500 até a fila subir (achado no `smoke.sh`). O guia
  manda rodar `up -d --wait`. Correção: conexão preguiçosa com retry, ou a fila no `/api/health`. ADR 0010.
- `npm ci` local avisa de install scripts bloqueados (`npm install-scripts ls`): revisar quais pacotes precisam.
