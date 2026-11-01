# 0029. Fila de jobs com Hangfire no Postgres

- **Status:** Aceito (fluxo autônomo, ADR 0026)
- **Data:** 2026-10-31

## Contexto
O sistema precisa rodar tarefas fora da requisição, com repetição, agendamento e um lugar para ver o que falhou: a
retenção da auditoria (ADR 0027) é a primeira, e outras virão (limpezas, avisos, rotinas). O usuário pediu algo como o
"BullMQ para .NET". O BullMQ é uma biblioteca de Node sobre Redis: usá-lo exigiria um Redis e um processo Node ao lado
da API. O `BackgroundService` que já existe (ADR 0025) serve para um laço fixo, não para jobs com retry e painel.

## Decisão
- **Hangfire 1.8** (`Hangfire.Core`, `Hangfire.NetCore` na Infrastructure; `Hangfire.AspNetCore` na API) com
  **`Hangfire.PostgreSql`**, no mesmo Postgres, schema `hangfire` (criado sozinho na primeira subida). Servidor de jobs
  dentro da API, 2 workers.
- **Jobs são classes da Application sem dependência do Hangfire** (ex.: `AuditRetentionJob.RunAsync()`), testadas
  como serviço. A Infrastructure declara a recorrência (`RecurringJobs`, `AddOrUpdate` idempotente na subida).
- **Primeiro job:** `audit-retention`, diário às 03:00 UTC, apaga a auditoria mais velha que `Jobs:AuditRetentionDays`
  (padrão 180, mínimo 7).
- **Painel em `/api/jobs`** (passa pelo Caddy), protegido por HTTP Basic com o usuário e a senha da cozinha
  (comparação em tempo constante): o painel é uma página do servidor e não recebe o JWT da SPA.

## Alternativas consideradas
- **BullMQ (Node + Redis):** dois serviços novos e outra linguagem só para a fila.
- **Quartz.NET:** agendamento forte, mas sem painel pronto nem a mesma ergonomia de fila com retry.
- **Fila no RabbitMQ que já existe:** daria o transporte, mas retry com espera, agendamento e painel teriam de ser
  escritos à mão.

## Consequências
- ✅ Retry automático, agendamento, recorrência e painel, sem serviço novo na instância.
- ✅ Um job novo é uma classe na Application e uma linha em `RecurringJobs`.
- ⚠️ Mais tabelas e consultas no Postgres (o Hangfire faz polling); com 2 workers, desprezível aqui.
- ⚠️ O painel aceita a senha da cozinha por Basic: só seguro sobre HTTPS (Caddy com domínio, ADR 0023).
