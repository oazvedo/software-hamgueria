# 023. Plan

- **ADRs aplicados:** 0008, 0013, 0028 (estende: navegador entra como fonte de logs e métricas). Sem ADR novo.

## Abordagem
Front: `shared/telemetry` gera `traceparent` por chamada (interceptor do `httpClient`), captura `error` e
`unhandledrejection` (até 5 por página) e mede Web Vitals com `web-vitals` 6.2.3 (import dinâmico), enviando por
`sendBeacon` (ou `fetch` com `keepalive`). API: `TelemetryController` público (`POST /api/telemetry/client-error` e
`/vitals`), rate limit de janela fixa por IP (`Telemetry:PermitPerMinute`, padrão 60), erro vira `LogWarning`
estruturado e vital vira o histograma `hamgueria.web_vitals` (tags nome, nota, rota). Handlers MSW aceitam e descartam.

## Verificação
CA-01: `BrowserTelemetryTests` (o span do servidor tem o trace id do cabeçalho) e `browserTelemetry.test.ts`.
CA-02, CA-03: `browserTelemetry.test.ts`; CA-02 também `BrowserTelemetryTests`. CA-04: `BrowserTelemetryTests`
(`/order/<uuid>?x=1` vira `/order/:param`) e `browserTelemetry.test.ts`. CA-05, CA-06: `BrowserTelemetryTests`.
