# Specs

Cada feature fica em `NNN-nome-em-kebab-case/`, com três documentos revisados em ordem (ADR 0002):

| Arquivo | Pergunta | Regra |
|---|---|---|
| `spec.md` | O quê? Por quê? | Sem tecnologia. Critérios de aceite testáveis (`CA-xx`), incluindo os de falha. |
| `plan.md` | Como? | Segue os ADRs. Decisão nova exige um ADR novo. |
| `tasks.md` | Em que passos? | Tasks de até ~30 min, cada uma com o comando de verificação. |

Para criar: skill `spec`. Para implementar: skill `implement-task`. Templates em [`_templates/`](_templates/).

**Status de uma spec:** `Rascunho` → `Aprovada` → `Em andamento` → `Concluída`.

## Roadmap
| # | Spec | Status |
|---|---|---|
| 001 | [Linha de base: o que o Hamgueria já faz](001-linha-de-base/spec.md) | Concluída (retroativa) |
| 002 | [Base de testes automatizados](002-base-de-testes/spec.md) | Concluída |
| 003 | [Validação de entrada e erros padronizados](003-validacao-problemdetails/spec.md) | Concluída |
| 004 | [Preparar o Hamgueria para rodar num servidor na nuvem](004-deploy-aws-ec2/spec.md) | Concluída |
| 005 | [Pedidos funcionam com a fila de mensagens fora do ar](005-fila-resiliente/spec.md) | Concluída |
| 006 | [Telas limpas, com a identidade dev só onde ela ajuda](006-limpeza-ui/spec.md) | Concluída |

## Lote autônomo (2026-10-31, ADR 0026)
Ordem de execução. Cada item vira uma spec com PR próprio para a `dev`. O que não couber vai para "Candidatos".

| # | Spec | O que resolve | Decisão já tomada |
|---|---|---|---|
| 007 | Comandas da mesa | A aba de comandas mostra só a comanda atual; uma mesa com várias comandas não as lista | Nav `~/comandas` lista todas as comandas da mesa (abertas e fechadas), com total e estado; `~/mesa` vira essa tela |
| 008 | Cardápio sem emoji e mais fácil de ler | Ícones de categoria em emoji; cardápio longo, em pilha, mostra pratos inativos | Ícones `lucide-react`; navegação por categoria fixa; prato inativo some do cardápio |
| 009 | Auditoria do banco | Ninguém sabe quem mudou um pedido, prato ou mesa, nem de onde | Triggers no PostgreSQL gravando antes/depois em JSONB, com autor (nome) e `frontend_url`; consulta para a cozinha |
| 010 | Observabilidade | Sem traces nem métricas; erro em produção só pelo log | OpenTelemetry (traces, métricas, logs) via OTLP; Aspire Dashboard no compose |
| 011 | Fila de jobs | Tarefas de fundo (limpeza, rotinas) não têm onde rodar com retry e painel | Hangfire com storage no Postgres (o equivalente .NET do BullMQ: retry, agendamento, painel); jobs de retenção da auditoria |
| 012 | Pedido seguro | Qualquer um com o id cancela; status aceita qualquer transição | Cancelamento só pela comanda dona; máquina de estados de status |
| 013 | CI de interface e imagens | `gate.sh` e os Dockerfiles não rodam no CI | Passo do gate no job `frontend`; job que constrói as duas imagens |
| 014 | Tema claro | O app só tem o tema escuro (`class="dark"` fixo); os tokens claros existem mas não são alcançáveis | Alternância claro/escuro no header, seguindo o sistema por padrão e lembrando a escolha; telas conferidas nos dois temas (pedido do usuário, 2026-10-31) |

## Candidatos a spec
Dívidas e ideias encontradas na adoção do fluxo (2026-10-31). Cada item vira uma spec, entra numa spec existente ou é
descartado. Ao aprovar uma spec que resolve um item, remova-o daqui.

| Candidato | Por quê | ADR afetado |
|---|---|---|
| Unit of Work | Pedido gravado e evento publicado não são atômicos; operações compostas não têm transação. | 0006, 0010 |
| Preço como `decimal` | `Dish.Price` é `double`: erros de arredondamento em dinheiro. | 0007 |
| Lint das fronteiras do FSD | A regra de imports do FSD não é verificada automaticamente. | 0011 |
| Paginação em `GET /api/order` | A lista da cozinha cresce sem limite. | 0006 |
| Esconder pratos inativos do cardápio | O cardápio mostra pratos desativados; o cliente só descobre no envio do pedido. | — |
| Rotas com `React.lazy` | As 6 páginas são importadas estaticamente em `app/router/routes.tsx`; o cliente baixa a tela da cozinha. (`vercel-react-best-practices` → `bundle-dynamic-imports`) | 0011 |
| Quebrar a `ProductionPage` | 375 linhas numa página; partes viram widgets/features (fila, por mesa, ações do pedido). | 0011 |
| Cancelamento só pelo dono da comanda | Qualquer pessoa com o id cancela o pedido (001/CA-12 ⚠️, ADR 0009). |  0009, 0014 |
| Transições de status válidas | `PATCH /status` aceita qualquer transição (ex.: `Cancelled` → `Done`); `UpdateAsync` volta o pedido para `Draft` mesmo se já servido. | — |
