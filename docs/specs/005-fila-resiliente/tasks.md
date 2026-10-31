# 005. Pedidos funcionam com a fila de mensagens fora do ar: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, a spec inteira de uma vez.
> **Pré-requisito:** ADR 0025 aceito (2026-10-31). Docker rodando. O `dotnet test` usa o Microsoft.Testing.Platform
> (`--project` e `--filter-class`).

## Fase 0: decisões
- [x] **T01. Aplicar o ADR 0025 nas referências**
  - Arquivos: `docs/adr/0010-eventos-de-pedido-no-rabbitmq.md` (status: substituído em parte por 0025),
    `CLAUDE.md` (Regras rápidas: eventos enfileirados e enviados em segundo plano),
    `.claude/skills/dotnet-feature/SKILL.md` (se citar publicação síncrona ou o `RabbitMqPublisher`)
  - Cobre: pré-requisito de todas
  - Verificação: `grep -n "0025" CLAUDE.md docs/adr/0010-eventos-de-pedido-no-rabbitmq.md` → uma linha em cada

## Fase 1: servidor
- [x] **T02. Fila de espera de eventos na Application**
  - Arquivos: `Application/Messaging/PendingOrderEvents.cs`, `Application/Messaging/QueuedOrderEventPublisher.cs`,
    `UnitTests/Application/Messaging/PendingOrderEventsTests.cs`, `UnitTests/Application/Messaging/QueuedOrderEventPublisherTests.cs`
  - Cobre: CA-06 (1001º descarta o mais antigo e loga o id), RN-03 (ordem), `RemoveHead` de item que já não é a cabeça
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.PendingOrderEventsTests" --filter-class "*.QueuedOrderEventPublisherTests"` → verde

- [x] **T03. Dispatcher em segundo plano, health check da fila e fim do `RabbitMqPublisher`**
  - Arquivos: `Infrastructure/Messaging/RabbitMqEventDispatcher.cs`, `Infrastructure/Messaging/RabbitMqHealthCheck.cs`,
    `Infrastructure/Messaging/RabbitMqPublisher.cs` (removido), `Infrastructure/InfrastructureModule.cs`,
    `Application/ApplicationModule.cs`, `FunctionalTests/Infrastructure/HamgueriaApiFactory.cs`
  - Desvio do plan: em vez de remover o dispatcher dos testes atuais (registrado por factory, removê-lo exigiria
    mexer nos `IHostedService` do host), o `HamgueriaApiFactory` aponta o envio para `127.0.0.1:1`: ele nunca alcança
    o RabbitMQ local do dev, e os eventos seguem no `FakeOrderEventPublisher`.
  - Cobre: RN-01, RN-06 (base de CA-02 a CA-05)
  - Verificação: `dotnet test --solution backend/SoftwareHamgueria.slnx` → verde (nada regrediu com o publisher novo)

- [x] **T04. `/api/health/details` e `/api/health` só com o banco**
  - Arquivos: `API/Health/HealthResponseWriter.cs` (pasta `Health`, não `Errors`: não é erro), `API/Program.cs`,
    `Infrastructure/InfrastructureModule.cs` (tags e fila como `Degraded`), `FunctionalTests/HealthEndpointTests.cs`
  - Cobre: CA-08 (banco parado: 503, `database: Unhealthy`, só as 4 chaves), RN-07
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.HealthEndpointTests"` → verde

- [x] **T05. Testes com o broker de verdade parando e voltando**
  - Arquivos: `FunctionalTests/SoftwareHamgueria.FunctionalTests.csproj` (+ `Testcontainers.RabbitMq` 4.16.0),
    `FunctionalTests/Infrastructure/BrokerApiFactory.cs`, `FunctionalTests/QueueResilienceTests.cs` (duas classes,
    `QueueDownAtStartupTests` e `QueueOutageWhileRunningTests`, cada uma com o próprio broker, rodando em paralelo),
    `FunctionalTests/Infrastructure/ApiClient.cs` (`UpdateOrderAsync`)
  - Cobre: CA-01, CA-02, CA-03, CA-04, CA-05, CA-07
  - **Achado e corrigido (decisão do usuário, 2026-10-31):** alterar pedido (`PUT /api/order/{id}`) dava 500 sempre
    na API real, com ou sem fila: o id do `OrderItem` é gerado no domínio e o EF o tratava como gerado pelo banco,
    fazendo `UPDATE` em vez de `INSERT` dos itens novos. Nenhum teste cobria o sucesso (só as recusas da 003).
    Correção: `ValueGeneratedNever()` em `OrderItemConfiguration` + migration `OrderItemIdGeneratedByDomain` (sem SQL,
    só o snapshot) + `OrderEndpointsTests` "005/CA-03: alterar o pedido troca os itens". O `dotnet-ef` 10.0.0 foi
    instalado como ferramenta global para gerar a migration.
  - Verificação: `dotnet test --project backend/tests/SoftwareHamgueria.FunctionalTests --filter-class "*.QueueDownAtStartupTests" --filter-class "*.QueueOutageWhileRunningTests" --filter-class "*.OrderEndpointsTests"` → verde

## Fase 2: interface
- [x] **T06. Handler MSW de `/api/health/details`**
  - Arquivos: `shared/mocks/handlers.ts`, `shared/mocks/handlers.test.ts`
  - Cobre: ADR 0013 para o endpoint novo
  - Verificação: `cd frontend && npm test -- handlers` → verde

## Fase 3: produção e documentação
- [x] **T07. Fumaça: reinício em qualquer ordem e fila caindo no meio**
  - Arquivos: `scripts/deploy/smoke.sh`
  - Cobre: CA-09 (restart sem `up --wait`, pedido 201 em até 60 s), CA-02/CA-03/CA-04 na pilha de produção
  - Verificação: `sh scripts/deploy/smoke.sh` → todos `OK`

- [x] **T08. Guia de deploy, backlog da 004 e roadmap**
  - Arquivos: `docs/deploy/aws-ec2.md` (sem o passo extra depois do reinício; `/api/health/details`),
    `docs/specs/004-deploy-aws-ec2/tasks.md` (item do `RabbitMqPublisher` resolvido pela 005), `docs/specs/README.md`
  - Cobre: impacto na linha de base da spec
  - Verificação: `grep -n "up -d --wait. de novo\|rode .up -d --wait" docs/deploy/aws-ec2.md` → vazio; suíte completa verde
    (`dotnet test --solution backend/SoftwareHamgueria.slnx`, `cd frontend && npm test && npm run lint`)

## Backlog
<!-- Achados da simplify e da revisão que ficam fora do escopo desta spec. Um item por linha, com o motivo. -->
- Outras entidades com id gerado no domínio (`Order`, `Dish`, `Table`, `ComandaClosure`) seguem com
  `ValueGeneratedOnAdd` no modelo. Hoje só são inseridas por `Add` (sem bug), mas o mesmo erro aparece se um dia
  forem adicionadas por navegação a uma entidade rastreada. Padronizar com `ValueGeneratedNever()` numa spec própria.
- Os testes com broker (`BrokerApiFactory`) sobem um Postgres e um RabbitMQ por cenário e somam ~15 s à suíte.
