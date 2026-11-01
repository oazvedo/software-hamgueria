# 009. Auditoria: Tasks

- [x] **T01. ADR 0027 e a migration com tabela, função e triggers** · `dotnet ef migrations add AddAuditTrail` + SQL
- [x] **T02. Contexto de auditoria e interceptor** (`IAuditContext`, `AuditContextInterceptor`, `HttpAuditContext`) · CA-02, CA-03
- [x] **T03. Leitura: entidade, repositório, serviço, validador e controller** · CA-01, CA-04 a CA-06 · `--filter-class "*.AuditTrailTests"`
- [x] **T04. Frontend: cabeçalhos, aba Histórico e mock** · CA-02, CA-07 · `npm test -- AuditLog auditHeaders`
- [x] **T05. Suíte completa e gate**

## Backlog
- Retenção da `audit.change_log` (job da spec 011) e, se crescer, particionamento mensal (o artigo de referência traz o modelo).
- O autor de cliente é declarado (cabeçalho). Quando houver identidade de cliente (candidato "Cancelamento só pelo dono"), usar o dono da comanda.
