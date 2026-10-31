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
