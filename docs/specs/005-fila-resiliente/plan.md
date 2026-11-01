# 005. Pedidos funcionam com a fila de mensagens fora do ar: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0006, 0010 (exceto conexão e publicação), 0013, 0016, 0022, 0023
- **ADRs novos propostos:** [0025](../../adr/0025-eventos-de-pedido-com-fila-de-espera-em-memoria.md) (fila de espera em
  memória + envio em segundo plano; substitui em parte o 0010)

## Abordagem
`Publish` passa a só enfileirar numa fila de espera da Application (`PendingOrderEvents`, FIFO, limite 1000, descarta o
mais antigo com log). Um `BackgroundService` na Infrastructure (`RabbitMqEventDispatcher`) conecta sob demanda, tenta
de novo a cada 5 s, publica com *publisher confirms* e só tira o evento da fila depois do confirm. O `RabbitMqPublisher`
atual sai. Um health check da fila entra só no novo `/api/health/details`; o `/api/health` continua só com o banco.
Os testes funcionais ganham um RabbitMQ do Testcontainers numa porta fixa, para parar e voltar o broker sem mudar de
endereço. O `smoke.sh` passa a reiniciar tudo sem `up --wait` (CA-09) e a derrubar a fila no meio (CA-03/CA-04).

## Modelo de dados
Sem mudança. Sem migration.

## Contrato da API
| Verbo | Rota | Acesso | Request | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/health` | Público | nenhum | igual à 004: 200 `Healthy` (só banco) | 503 `Unhealthy` |
| GET | `/api/health/details` | Público | nenhum | 200 JSON (abaixo), mesmo com a fila fora | 503 com o mesmo JSON se o banco estiver fora |

```json
{ "status": "Degraded", "database": "Healthy", "queue": "Unhealthy", "pendingEvents": 3 }
```
`status`: `Healthy` (tudo no ar), `Degraded` (só a fila fora), `Unhealthy` (banco fora). Sem `description`, exceção,
host ou usuário (RN-08). Rotas de pedido: sem mudança de contrato; com a fila fora respondem igual a hoje com ela no ar.

Eventos RabbitMQ: mesmos exchange (`hamgueria.orders`), routing keys e corpo do ADR 0010. Entrega pelo menos uma vez.

## Backend: arquivos
- `Application/Messaging/PendingOrderEvents.cs` (novo): `record PendingOrderEvent(string RoutingKey, string Message)`;
  `Enqueue`, `TryPeek`, `RemoveHead(item)` (remove só se o item ainda é a cabeça), `Count`, `WaitAsync(ct)` (sinal de
  item novo), limite no construtor (1000 por padrão) e `ILogger` para o descarte.
- `Application/Messaging/QueuedOrderEventPublisher.cs` (novo): `IOrderEventPublisher` que chama `Enqueue`.
- `Application/ApplicationModule.cs`: `PendingOrderEvents` e `IOrderEventPublisher → QueuedOrderEventPublisher` singletons.
- `Infrastructure/Messaging/RabbitMqEventDispatcher.cs` (novo, `BackgroundService`): conexão sob demanda,
  `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)`, declara o
  exchange, laço peek → publica (timeout 5 s) → `RemoveHead`; falha de conexão/timeout: fecha, `IsConnected = false`,
  espera 5 s; `nack`: loga e descarta. `StopAsync` loga quantos ficaram. Opções em `RabbitMq:RetryDelaySeconds` e
  `RabbitMq:PublishTimeoutSeconds` (padrão 5), para os testes usarem 1.
- `Infrastructure/Messaging/RabbitMqHealthCheck.cs` (novo): `Healthy` se `IsConnected`, senão `Unhealthy`; `data`
  com `pendingEvents`.
- `Infrastructure/Messaging/RabbitMqPublisher.cs`: removido.
- `Infrastructure/InfrastructureModule.cs`: `AddSingleton<RabbitMqEventDispatcher>` + `AddHostedService(sp => ...)`;
  health checks com tags (`db` no banco, `queue` na fila). Sai o registro do `RabbitMqPublisher`.
- `API/Errors/HealthResponseWriter.cs` (novo): escreve o JSON enxuto do `/details`.
- `API/Program.cs`: `MapHealthChecks("/api/health", Predicate = tag db)` e `MapHealthChecks("/api/health/details",
  ResponseWriter = HealthResponseWriter, ResultStatusCodes: Degraded → 200)`.
- Testes:
  - `UnitTests/Application/Messaging/PendingOrderEventsTests.cs` (novo): ordem FIFO, limite (1001º descarta o mais
    antigo e loga o id), `RemoveHead` com item que já não é a cabeça não remove nada.
  - `UnitTests/Application/Messaging/QueuedOrderEventPublisherTests.cs` (novo): `Publish` enfileira e nunca lança.
  - `FunctionalTests/SoftwareHamgueria.FunctionalTests.csproj`: + `Testcontainers.RabbitMq` (versão exata, a mesma
    família 4.16.0).
  - `FunctionalTests/Infrastructure/HamgueriaApiFactory.cs`: remove o `RabbitMqEventDispatcher` dos hosted services
    (os testes atuais seguem com o `FakeOrderEventPublisher`, sem broker).
  - `FunctionalTests/Infrastructure/BrokerApiFactory.cs` (novo): API com Postgres e RabbitMQ do Testcontainers, o
    RabbitMQ numa porta livre fixa (`WithPortBinding(porta, 5672)`), dispatcher real, `RetryDelaySeconds=1`; método
    para parar/subir o broker e um leitor de teste ligado ao exchange (`order.#`).
  - `FunctionalTests/QueueResilienceTests.cs` (novo): CA-01 a CA-05, CA-07; e `HealthEndpointTests` ganha o CA-08.

## Frontend: arquivos
- `shared/mocks/handlers.ts`: `GET /api/health/details` → `{ status: 'Healthy', database: 'Healthy', queue: 'Healthy',
  pendingEvents: 0 }` (ADR 0013), com teste em `handlers.test.ts`. Sem tela nova.

## Raiz e documentação
- `scripts/deploy/smoke.sh`: depois do `restart`, sem `up --wait`, pedido criado em até 60 s (CA-09); fila parada:
  pedido 201 e `/details` com `queue: Unhealthy` e `pendingEvents ≥ 1`; fila de volta: `pendingEvents: 0` em até 60 s.
- `docs/deploy/aws-ec2.md`: sai o aviso de "rode `up -d --wait` depois de reiniciar"; entra o `/api/health/details`.
- `docs/specs/004-deploy-aws-ec2/tasks.md`: item do `RabbitMqPublisher` no Backlog marcado como resolvido pela 005.
- `docs/adr/0010-...md`: status "Substituído em parte por 0025"; `docs/adr/README.md`; `CLAUDE.md` (Regras rápidas:
  eventos enfileirados e enviados em segundo plano); skill `dotnet-feature` se citar publicação síncrona.

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | Funcional | `QueueResilienceTests` (`005/CA-01`): broker no ar, criar/alterar/cancelar chegam ao leitor de teste |
| CA-02 | Funcional + script | `BrokerApiFactory` sobe com o broker parado: cardápio, comanda, pedido e login respondem 200 |
| CA-03 | Funcional | broker parado: criar, alterar e cancelar → 2xx; `/details` com `pendingEvents` = 3 |
| CA-04 | Funcional | broker volta: o leitor recebe os 3 na ordem; `pendingEvents` = 0 |
| CA-05 | Funcional | conectado → para o broker → pedido → volta → evento chega |
| CA-06 | Unidade | `PendingOrderEventsTests` (`005/CA-06`, limite 1000, log com o id) |
| CA-07 | Funcional | broker parado: `/api/health` 200 `Healthy`; `/details` 200 `Degraded`, `queue: Unhealthy` |
| CA-08 | Funcional | `HealthEndpointTests` (`005/CA-08`): banco parado, `/details` 503 com `database: Unhealthy`, corpo só com as 4 chaves |
| CA-09 | Script | `smoke.sh`: `restart` sem `up --wait`, pedido 201 em até 60 s |

## Riscos e decisões pendentes
- Testes com broker parando e voltando podem ficar lentos ou instáveis: `RetryDelaySeconds=1`, esperas com prazo
  (até 30 s) e checagem por condição, nunca `sleep` fixo.
- `WithPortBinding` com porta fixa pode colidir no CI: a porta é escolhida livre na hora (`TcpListener` na porta 0).
- Entrega pelo menos uma vez pode duplicar evento (ADR 0025). Sem consumidor hoje, sem impacto.
- Memória: 1000 eventos de ~60 bytes, desprezível.
