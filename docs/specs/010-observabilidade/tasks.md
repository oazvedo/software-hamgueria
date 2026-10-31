# 010. Observabilidade: Tasks

- [x] **T01. Instrumentos próprios e spans** (`HamgueriaTelemetry`, `OrderService`, `PendingOrderEvents`, dispatcher) · CA-02, CA-03
- [x] **T02. `AddObservability` na API** (traces, métricas, logs, JSON fora de dev) · CA-01, CA-05, CA-06
- [x] **T03. Aspire Dashboard nos composes e `.env.example`** · CA-04, CA-06
- [x] **T04. Testes** (`TelemetryTests`, `ObservabilityTests`) e conferência no painel

## Backlog
- Telemetria do navegador (Web Vitals, erros de JS) e propagação de `traceparent` do front para a API.
- Alertas (ex.: `hamgueria.order_events.pending` alto por mais de 5 min) quando houver backend de métricas com alerta.
- Teste do descarte por `nack` (`reason=nack`): o broker real só recusa em casos raros (fila em limite com `reject-publish`); fica para quando houver um broker de teste configurável.
