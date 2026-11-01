# 011. Jobs: Plan

- **Spec:** [spec.md](spec.md) · **ADR novo:** [0029](../../adr/0029-fila-de-jobs-com-hangfire.md)

## Abordagem
Hangfire 1.8.25 + Hangfire.PostgreSql 1.21.1. `AuditRetentionJob` na Application (com `TimeProvider`),
`IAuditRepository.DeleteOlderThanAsync` (`ExecuteDeleteAsync`), `JobsRegistration` + `RecurringJobs` na
Infrastructure, painel em `/api/jobs` com `AdminBasicAuthFilter` na API.

## Verificação
| CA | Como |
|---|---|
| CA-01 | Funcional: `JobsTests` (recorrente com cron `0 3 * * *`) |
| CA-02 | Unidade: `AuditRetentionJobTests`; funcional: linhas de 400 dias e de 1 dia, só a velha some |
| CA-03 | Unidade: `AuditRetentionJobTests` (6, 0, -10, abc) |
| CA-04 | Funcional: `JobsTests` (sem credencial, senha errada, usuário errado: 401 com `WWW-Authenticate`; admin: 200) |
