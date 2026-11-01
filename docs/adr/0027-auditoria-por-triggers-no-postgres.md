# 0027. Auditoria do banco por triggers no PostgreSQL, com autor e origem

- **Status:** Aceito (fluxo autônomo, ADR 0026)
- **Data:** 2026-10-31

## Contexto
Não há como saber quem mudou um pedido, prato, mesa ou comanda, nem de qual tela a mudança saiu. Auditoria na
aplicação (hook no `SaveChanges`) perde o que muda por SQL direto e depende de cada caminho lembrar de gravar. O usuário
pediu a abordagem por triggers do artigo "PostgreSQL triggers for audit" (OneUptime, 2026-01-30), com o autor em nome e
a URL do frontend.

## Decisão
- **Schema `audit`, tabela `audit.change_log`:** `id`, `table_name`, `operation` (INSERT/UPDATE/DELETE), `row_id`,
  `old_values`/`new_values` (JSONB), `changed_fields` (text[]), `transaction_id` (`txid_current()`), `author_name`,
  `frontend_url`, `request_id`, `db_user`, `created_at` (`clock_timestamp()`). Índices por `(table_name, row_id)` e
  `created_at`.
- **Uma função genérica `audit.log_changes()`** (`SECURITY DEFINER`, `search_path` fixo), `AFTER ... FOR EACH ROW` em
  `Orders`, `OrderItems`, `Dishes`, `Tables` e `ComandaClosures`. No UPDATE grava só as colunas que mudaram, ignorando
  as passadas como argumento (`UpdatedAt`); se nada mudou, não grava. Tabela nova entra com uma linha de migration.
- **Contexto pela sessão do Postgres:** a função lê `current_setting('app.author_name' | 'app.frontend_url' |
  'app.request_id', true)`. Um `DbConnectionInterceptor` (Infrastructure) grava esses valores com `set_config(...,
  false)` a cada abertura de conexão, vindos de `IAuditContext` (porta na Application). O Npgsql faz `DISCARD ALL` ao
  devolver a conexão ao pool, então um valor não vaza para a próxima requisição.
- **Quem é o autor:** a cozinha logada é o nome do usuário do token; o cliente é o nome da comanda, enviado pelo
  frontend no cabeçalho `X-Author-Name` (codificado com `encodeURIComponent`). A origem é o `X-Frontend-Url` (a página
  aberta), com o `Referer` como reserva. Sem requisição (migrations, seed, jobs), o autor é `sistema`. O
  `request_id` é o mesmo `traceId` dos ProblemDetails (ADR 0022). Os cabeçalhos são informativos (o cliente pode
  mentir o próprio nome); não servem para autorização.
- **Leitura:** a tabela é mapeada no EF só para leitura (`ExcludeFromMigrations`), criada por SQL na migration.
  `GET /api/audit` (Admin) pagina por tabela e registro. A adaptação HTTP (`HttpAuditContext`) é registrada no
  `Program.cs`, como as outras peças só da API (handler de exceção).

## Alternativas consideradas
- **Interceptor de `SaveChanges` gravando a auditoria pela aplicação:** não vê SQL direto e duplica a lógica de diff.
- **Extensões prontas (pgaudit, temporal tables):** pgaudit audita comandos, não linhas com antes/depois; temporal
  tables não existem nativas no Postgres 16.
- **Particionar `change_log` por mês já agora:** o volume do projeto não pede; a retenção fica num job (spec 011).

## Consequências
- ✅ Toda escrita nas 5 tabelas fica registrada, com antes, depois, autor e tela, inclusive mudanças fora da API.
- ✅ Consulta de histórico por registro para a cozinha.
- ⚠️ Uma ida extra ao banco por abertura de conexão (o `set_config`) e uma linha de auditoria por linha alterada.
- ⚠️ `author_name` de cliente é declarado, não autenticado. A tabela cresce sem limite até a retenção existir.
