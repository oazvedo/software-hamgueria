# 009. Auditoria: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0005, 0006, 0008, 0009, 0011, 0013, 0016, 0021, 0022, 0026
- **ADR novo:** [0027](../../adr/0027-auditoria-por-triggers-no-postgres.md) (triggers + contexto pela sessão)

## Abordagem
Migration `AddAuditTrail` com SQL: schema `audit`, tabela `change_log`, função `audit.log_changes()` e um trigger
`AFTER ... FOR EACH ROW` em cada tabela, ignorando `UpdatedAt`. `AuditContextInterceptor` (Infrastructure) faz
`set_config('app.author_name' | 'app.frontend_url' | 'app.request_id')` ao abrir a conexão, com valores de
`IAuditContext` (Application): `HttpAuditContext` na API (usuário do token, ou `X-Author-Name` decodificado; `X-Frontend-Url`
ou `Referer`; `traceId`), `SystemAuditContext` fora de requisição. Leitura por `GET /api/audit` (Admin), com
`AuditQueryValidator` (tabela da lista, página ≥ 1, tamanho 1–200) e `hasMore` pelo truque de buscar um a mais. No
front, `httpClient` manda os dois cabeçalhos (`auditHeaders`, nome sincronizado pelo app); a cozinha ganha a aba
"Histórico" (`widgets/audit-log`). Handler MSW de `/api/audit` monta o histórico dos pedidos simulados.

## Contrato
| Verbo | Rota | Acesso | Query | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/audit` | Admin | `table`, `rowId`, `page`, `pageSize` | 200 `AuditPageDto` | 400 validação, 401, 403 |

## Verificação
| CA | Como |
|---|---|
| CA-01 a CA-06 | Funcional: `AuditTrailTests` (Postgres real com a migration e os triggers) |
| CA-02 | Interface: `auditHeaders.test.ts` (nome codificado, URL da página) |
| CA-07, CA-05 | Interface: `AuditLog.test.tsx` |
