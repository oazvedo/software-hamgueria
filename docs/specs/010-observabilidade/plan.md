# 010. Observabilidade: Plan

- **Spec:** [spec.md](spec.md) · **ADR novo:** [0028](../../adr/0028-observabilidade-com-opentelemetry.md)

## Abordagem
Pacotes OpenTelemetry 1.20.1 e `Npgsql.OpenTelemetry` 10.0.3 na API. `AddObservability()` no `Program.cs`. Instrumentos
próprios em `HamgueriaTelemetry` (Application, só BCL), usados no `OrderService`, na `PendingOrderEvents` e no
`RabbitMqEventDispatcher` (span `publish order event`). Aspire Dashboard nos composes. Testes zeram o endpoint OTLP.

## Verificação
| CA | Como |
|---|---|
| CA-01 | Funcional: `ObservabilityTests` (span de servidor com rota e span do Npgsql no mesmo trace); manual: print do painel |
| CA-02 | Funcional: `ObservabilityTests` (`MeterListener`) |
| CA-03 | Unidade: `TelemetryTests` |
| CA-04 | Manual: `docker compose up -d` + API em dev, traces no painel |
| CA-05 | Toda a suíte roda sem coletor |
| CA-06 | `docker compose -f docker-compose.prod.yml --profile observability config` (porta só em `127.0.0.1`) |
