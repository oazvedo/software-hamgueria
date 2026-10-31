# 0028. Observabilidade com OpenTelemetry e Aspire Dashboard

- **Status:** Aceito (fluxo autônomo, ADR 0026)
- **Data:** 2026-10-31

## Contexto
Em produção (ADR 0023), um erro só aparece no log do container, e não há como ver latência, consultas lentas ou o que
aconteceu dentro de uma requisição. A fila de eventos em memória (ADR 0025) e a auditoria (ADR 0027) também precisam de
números (eventos esperando, descartados).

## Decisão
- **OpenTelemetry na API** (`API/Observability/ObservabilityExtensions.cs`): traces de ASP.NET Core, HttpClient, Npgsql
  e do envio de eventos; métricas de ASP.NET Core, HttpClient, runtime, Npgsql e as próprias; logs pelo provider do
  OpenTelemetry. Recurso `service.name = hamgueria-api`. O `/api/health` não gera trace (é sonda).
- **Instrumentos próprios** em `Application/Telemetry/HamgueriaTelemetry` só com tipos do BCL (`ActivitySource`,
  `Meter`): `hamgueria.orders.created`, `hamgueria.order_events.published`, `hamgueria.order_events.dropped` (com o
  motivo) e `hamgueria.order_events.pending` (gauge).
- **Exportação OTLP só com `OTEL_EXPORTER_OTLP_ENDPOINT`** definido: o `appsettings.Development.json` aponta para
  `localhost:4317`; os testes zeram; produção decide pelo `.env`.
- **Aspire Dashboard** (`mcr.microsoft.com/dotnet/aspire-dashboard:9.5`) como coletor e tela: no compose de dev (UI
  `localhost:18888`, OTLP `4317`, sem login) e no de produção como perfil opcional `observability`, publicado só no
  loopback da instância (acesso por túnel SSH, com o token de login do próprio painel).
- **Fora de Development, log em JSON** no console, para o `docker compose logs` ser filtrável.

## Alternativas consideradas
- **Prometheus + Grafana + Tempo + Loki:** o padrão de mercado, mas são quatro serviços para uma instância `t3.small`.
  O OTLP deixa trocar depois sem mudar o código.
- **Serilog com sinks:** resolve logs, não traces nem métricas.
- **Serviço gerenciado (CloudWatch, Datadog):** custo e conta externa; o usuário decide quando quiser.

## Consequências
- ✅ Um trace por requisição com as consultas ao banco; métricas da fila de eventos; logs estruturados.
- ✅ Trocar de backend de telemetria é configuração (OTLP).
- ⚠️ O Aspire Dashboard guarda tudo em memória: reiniciar apaga o histórico. Serve para investigar, não para guardar.
- ⚠️ Instrumentação sempre ligada tem custo pequeno de CPU por requisição.
