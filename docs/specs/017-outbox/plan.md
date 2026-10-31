# 017. Plan

- **ADR novo:** [0031](../../adr/0031-outbox-transacional.md) (substitui o 0025; muda o "publique depois" do 0010)

## Abordagem
`OutboxMessage` (Domain) + configuração e migration `AddOutbox`; `OutboxOrderEventPublisher` (scoped) adiciona a linha
sem salvar; `OrderService` publica antes da escrita; `RabbitMqEventDispatcher` lê lotes com `FOR UPDATE SKIP LOCKED`;
health check conta o outbox ao vivo; job `outbox-retention`. Saem `PendingOrderEvents`, `QueuedOrderEventPublisher` e
o `FakeOrderEventPublisher` dos testes (os testes leem a tabela real com `OutboxProbe`).

## Verificação
CA-01: `OutboxTests`, e os testes antigos de evento (`OrderEndpointsTests`, `AuthEndpointsTests`) agora pela tabela real.
CA-02: `OutboxTests` (trigger de teste que faz o insert falhar). CA-03: `OutboxDeliveryTests` (broker real) e os
cenários da 005 (`QueueResilienceTests`). CA-04: `OutboxRetentionTests`. `smoke.sh` OK.
