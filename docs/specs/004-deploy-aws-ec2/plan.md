# 004. Preparar o Hamgueria para rodar num servidor na nuvem: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0005, 0008, 0009, 0010, 0011, 0013, 0016, 0017, 0022
- **ADRs novos propostos:** [0023](../../adr/0023-deploy-em-ec2-com-docker-compose-e-caddy.md) (EC2 + compose +
  Caddy), [0024](../../adr/0024-segredos-por-variaveis-de-ambiente-validados-no-startup.md) (segredos por ambiente,
  validados no startup)

## Abordagem
Segredos saem do `appsettings.json`: os valores de dev vão para o `appsettings.Development.json` (portas novas) e os de
produção chegam por variável de ambiente a partir do `.env` (ADR 0024). Fora de `Development`, a API valida a
configuração antes do `Build()` e não sobe se faltar segredo ou se ele for fraco. Duas imagens novas: a API
(`backend/Dockerfile`) e a tela servida pelo Caddy, que também faz o proxy de `/api` e o HTTPS (`frontend/Dockerfile`).
O `docker-compose.prod.yml` na raiz junta tudo com só o `web` exposto (ADR 0023). A API ganha `GET /api/health` com
checagem do banco. O compose de dev passa a usar 5433/5673/15673, lidas de `backend/.env`. Um script de fumaça sobe a
pilha de produção localmente e confere os CAs de infraestrutura.

## Modelo de dados
Sem mudança. Sem migration.

## Contrato da API
| Verbo | Rota | Acesso | Request | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/health` | Público | nenhum | 200 `text/plain` `Healthy` | 503 `Unhealthy` (sem detalhe) |

Mapeado com `app.MapHealthChecks("/api/health")` + `AddDbContextCheck<AppDbContext>()`, não é controller: é um
endpoint de infraestrutura, fora da regra de recursos do ADR 0008. Fica sob `/api` para o Caddy repassar sem regra
extra. Eventos RabbitMQ: sem mudança.

## Configuração
| Chave ASP.NET | `appsettings.json` | `appsettings.Development.json` | Produção (`.env` → compose) |
|---|---|---|---|
| `ConnectionStrings:Postgres` | (sem) | `Host=localhost;Port=5433;...;Password=hamgueria123` | montada de `POSTGRES_PASSWORD`, host `postgres` |
| `RabbitMq:Host` / `Port` | `localhost` / `5672` | `localhost` / `5673` | `rabbitmq` / `5672` |
| `RabbitMq:Username` / `Password` | (sem) | `guest` / `guest` | `RABBITMQ_USER` / `RABBITMQ_PASSWORD` |
| `Auth:JwtSecret` | (sem) | chave de dev atual | `AUTH_JWT_SECRET` (`openssl rand -base64 48`) |
| `Auth:AdminUsername` | `admin` | `admin` | `AUTH_ADMIN_USERNAME` |
| `Auth:AdminPassword` | (sem) | `admin123` | `AUTH_ADMIN_PASSWORD` |
| `Auth:Issuer` / `Audience` | iguais a hoje | | |

Arquivos de ambiente:
- `backend/.env.example` → `backend/.env` (dev, criado nesta spec, ignorado): `POSTGRES_PORT=5433`,
  `RABBITMQ_PORT=5673`, `RABBITMQ_MANAGEMENT_PORT=15673`, usuário e senha locais. O `backend/docker-compose.yml` usa
  `${VAR:-padrão}`, então funciona mesmo sem o `.env`.
- `.env.example` (raiz) → `.env` no servidor: `SITE_ADDRESS`, `HTTP_PORT`, `HTTPS_PORT`, `POSTGRES_PASSWORD`,
  `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `AUTH_ADMIN_USERNAME`, `AUTH_ADMIN_PASSWORD`, `AUTH_JWT_SECRET`. O compose usa
  `${VAR:?mensagem}` nos segredos, então nem chega a subir a API sem eles (a validação do startup é a segunda barreira).
- Os testes funcionais rodam em `Development` (padrão do `WebApplicationFactory`) e continuam usando `admin123`.

## Backend: arquivos
- `Application/Configuration/RequiredSettings.cs` (novo): `static IReadOnlyList<string> Check(IConfiguration)`, regras
  do ADR 0024. Mensagens com o nome da chave e a regra, sem valor.
- `Application/Services/AuthService.cs`: sem `?? "admin"` e `?? "admin123"`; sem usuário ou senha configurados, o
  login é negado (devolve `null`).
- `API/Program.cs`: `RequiredSettings.Check` fora de `Development`, antes do `Build()`; `AddHealthChecks()
  .AddDbContextCheck<AppDbContext>()`; `MapHealthChecks("/api/health")`.
- `API/SoftwareHamgueria.API.csproj`: + `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (versão exata).
- `API/appsettings.json` (sem segredos) e `API/appsettings.Development.json` (valores de dev, portas novas).
- `Infrastructure/Data/AppDbContextFactory.cs`: lê `ConnectionStrings__Postgres` do ambiente; sem ele, usa a de dev
  na 5433 (só design-time do `dotnet ef`).
- `backend/Dockerfile` (novo) e `backend/.dockerignore` (novo: `bin/`, `obj/`, `tests/`, `.env`).
- `backend/docker-compose.yml`: portas e credenciais por `${VAR:-padrão}`; `backend/.env.example` (novo).
- Testes:
  - `UnitTests/Application/RequiredSettingsTests.cs` (novo): CA-04, CA-05 (limites 11/12 e 31/32, `admin123`,
    `guest`, chave ausente), e que nenhuma mensagem contém o valor.
  - `UnitTests/Application/AuthServiceTests.cs`: CA-10 (sem senha configurada, login negado).
  - `FunctionalTests/HealthEndpointTests.cs` (novo): CA-09 sucesso (200 `Healthy`) e falha (factory própria com
    Postgres do Testcontainers parado depois do startup → 503 `Unhealthy`, corpo sem detalhe).
  - `FunctionalTests/ProductionConfigTests.cs` (novo): CA-04 de ponta, host em `Production` sem
    `Auth:AdminPassword` → a criação do servidor lança e a mensagem cita a chave.

## Frontend: arquivos
- `frontend/Dockerfile` (novo, build com `VITE_ENABLE_MOCKS=false`), `frontend/.dockerignore` (novo),
  `frontend/Caddyfile` (novo: `{$SITE_ADDRESS}`, `handle /api/*` → `reverse_proxy api:8080`, resto `try_files
  {path} /index.html` + `file_server`, `encode gzip zstd`).
- `shared/mocks/handlers.ts`: `GET /api/health` → `Healthy` (ADR 0013), com teste no Vitest.
- `frontend/.env.example`: só comentário do proxy, sem mudança de porta (a API continua na 5262).
- Sem tela nova ou alterada: sem auditoria Visant.

## Raiz e documentação
- `docker-compose.prod.yml` (novo), `.env.example` (novo). O `.gitignore` já ignora `.env` e libera `!.env.example`.
- `scripts/deploy/smoke.sh` (novo): com um `.env` de teste (`SITE_ADDRESS=:80`, `HTTP_PORT=8088`), sobe a pilha com
  um nome de projeto próprio, espera o `web`, confere tela, `/api/dish`, `/api/health`, `/swagger` 404, as portas
  publicadas só no `web`, cria um pedido, reinicia e confere que o pedido persiste, e derruba tudo (volumes inclusos).
- `docs/deploy/aws-ec2.md` (novo): instância, security group, instalar Docker, clonar, `.env` (`chmod 600`), subir,
  domínio e HTTPS, atualizar, `pg_dump` manual, ler logs.
- `README.md` e `CLAUDE.md`: portas novas (5433/5673/15673), comando do e2e real, link do guia.
- `docs/specs/README.md`: roadmap 004; remove o candidato "Segredos fora do `appsettings`".

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | Manual + suíte | `docker compose up -d` em `backend/`, `docker compose ps` mostra 5433/5673/15673, `dotnet run` da API e `curl :5262/api/dish`; suíte completa verde |
| CA-02 | Manual | nesta máquina a 5432 já está ocupada (spec 002): subir o compose de dev sem erro de porta |
| CA-03 | Script | `scripts/deploy/smoke.sh`: tela e `/api/dish` pelo `web`, login com a senha do `.env` de teste |
| CA-04 | Unidade + funcional | `RequiredSettingsTests`, `ProductionConfigTests` (`004/CA-04`) |
| CA-05 | Unidade | `RequiredSettingsTests` (`004/CA-05`, limites 11/12 e 31/32) |
| CA-06 | Script | `smoke.sh`: `docker compose ps --format json` sem portas fora do `web`; `/swagger` → 404 |
| CA-07 | Script | `smoke.sh`: cria pedido, `docker compose restart` e `up -d --build`, pedido continua |
| CA-08 | Script + build | `smoke.sh` envia pedido pelo `web`; o bundle de produção não registra o service worker do MSW (`VITE_ENABLE_MOCKS=false`) |
| CA-09 | Funcional | `HealthEndpointTests` (`004/CA-09`, sucesso e banco parado) |
| CA-10 | Unidade + manual | `AuthServiceTests` (`004/CA-10`); `git check-ignore .env backend/.env`; `grep -rn admin123 backend/src --include=*.cs` vazio |

## Riscos e decisões pendentes
- Domínio ainda indefinido: o primeiro deploy pode ficar em HTTP pelo IP (guia avisa). Trocar é só mudar `SITE_ADDRESS`.
- `RabbitMqPublisher` conecta no construtor: se a fila demorar, as rotas de pedido falham. `depends_on` + `healthcheck`
  cobrem o `up`, mas **não** o `restart` nem o reboot da instância (confirmado no `smoke.sh`, ver Backlog do tasks.md).
- O `Migrate()` no startup roda em produção sem confirmação (ADR 0005, risco já aceito).
- CI não constrói as imagens. Candidato a backlog: job que roda `docker build` dos dois Dockerfiles.
- Imagens base sem digest fixo (`aspnet:10.0`, `caddy:2-alpine`): aceitável nesta fase, fica no backlog.
