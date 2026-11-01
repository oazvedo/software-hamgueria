# 011. Jobs: Tasks

- [x] **T01. `AuditRetentionJob` e `DeleteOlderThanAsync`** · CA-02, CA-03
- [x] **T02. Hangfire no Postgres e recorrência** · CA-01
- [x] **T03. Painel em `/api/jobs` com Basic da cozinha** · CA-04
- [x] **T04. Testes e documentação** (ADR 0029, guia de deploy)

## Backlog
- Rotina para comandas abertas há mais de um dia (decidir a regra: fechar ou só avisar a cozinha).
- Traces do Hangfire no OpenTelemetry (ADR 0028), quando houver mais de um job.
- ~~Teste instável raro na suíte funcional (1 falha em 10 rodadas depois da correção da porta do broker, causa não capturada). Se voltar no CI, rodar com `--output detailed` e anotar aqui.~~ Resolvido na spec 026.
