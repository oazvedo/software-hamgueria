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
| 007 | [Todas as comandas da mesa num lugar só](007-comandas-da-mesa/spec.md) | Concluída |
| 008 | [Cardápio sem emoji e mais fácil de percorrer](008-cardapio/spec.md) | Concluída |
| 009 | [Auditoria: quem mudou o quê, quando e de qual tela](009-auditoria/spec.md) | Concluída |
| 010 | [Ver o que acontece dentro do sistema](010-observabilidade/spec.md) | Concluída |
| 011 | [Tarefas de fundo com repetição e painel](011-jobs/spec.md) | Concluída |
| 012 | [Só o dono muda o pedido, e status não anda para trás](012-pedido-seguro/spec.md) | Concluída |
| 013 | [O CI cobra o gate de interface e as imagens de produção](013-ci-gate-e-imagens/spec.md) | Concluída |
| 014 | [Tema claro](014-tema-claro/spec.md) | Concluída |

## Lote autônomo (2026-10-31, ADR 0026): concluído
Ordem de execução, todas integradas na `dev` (PRs #7 a #15). Cada item vira uma spec com PR próprio para a `dev`. O que não couber vai para "Candidatos".

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

## Lote 2 (2026-10-31, ADR 0026): "corrige todos em fila"
| # | Spec | O que resolve | Decisão já tomada |
|---|---|---|---|
| 015 | Cliente cancela e altera o pedido | A API e o token de dono existem (012), a tela não | Botões no acompanhamento do pedido, só com o token do aparelho e status não final |
| 016 | Preço em `decimal` | `double` arredonda errado em dinheiro | `numeric(10,2)` no banco, `decimal` no domínio e nos DTOs |
| 017 | Pedido e evento na mesma transação | Pedido gravado e evento publicado não são atômicos | Outbox no Postgres (substitui a fila em memória do ADR 0025) |
| 018 | Dependências vulneráveis | `Cryptography.Pkcs`/`Xml` 9.0.0 e `Newtonsoft.Json` 11 (alta) | Fixar versões corrigidas como referência direta |
| 019 | Seed concorrente e comanda de mesa inexistente | Dois bugs pequenos (002, 003) | Advisory lock no seed; 404 ao fechar comanda de mesa que não existe |
| 020 | Lista da cozinha limitada | `GET /api/order` cresce sem limite | Ativos + finalizados das últimas 24 h, com teto |
| 021 | Bundle e tela da cozinha | Rotas carregadas de uma vez; `ProductionPage` com 375+ linhas | `React.lazy` por rota; a cozinha vira widgets |
| 022 | Fronteiras do FSD verificadas | Regra de imports só no olho | Script `fsd-lint` no gate e no CI |
| 023 | Telemetria do navegador | Erros de JS e Web Vitals invisíveis | `traceparent` nas chamadas, erros e vitals enviados à API e para o OpenTelemetry |
| 024 | Menores | Itens soltos dos backlogs | Imagens por digest, `GET /api/dish?ativos=true`, `ValueGeneratedNever`, scripts do npm |
| 025 | Comandas esquecidas | Comanda aberta há horas sem ninguém ver | Aviso na cozinha (não fecha sozinho: fechar é irreversível) |
| 026 | Teste instável | 1 falha rara em ~10 rodadas | Rodar em repetição, capturar e corrigir a causa |

Também feito: check `images` obrigatório no ruleset da `main` (junto de `backend`, `frontend`, `e2e`, `convencao`).

## Candidatos a spec
Dívidas e ideias encontradas na adoção do fluxo (2026-10-31). Cada item vira uma spec, entra numa spec existente ou é
descartado. Ao aprovar uma spec que resolve um item, remova-o daqui.

| Candidato | Por quê | ADR afetado |
|---|---|---|
| Unit of Work | Pedido gravado e evento publicado não são atômicos; operações compostas não têm transação. | 0006, 0010 |
| Preço como `decimal` | `Dish.Price` é `double`: erros de arredondamento em dinheiro. | 0007 |
| Lint das fronteiras do FSD | A regra de imports do FSD não é verificada automaticamente. | 0011 |
| Paginação em `GET /api/order` | A lista da cozinha cresce sem limite. | 0006 |
| Rotas com `React.lazy` | As 6 páginas são importadas estaticamente em `app/router/routes.tsx`; o cliente baixa a tela da cozinha. (`vercel-react-best-practices` → `bundle-dynamic-imports`) | 0011 |
| Quebrar a `ProductionPage` | 375 linhas numa página; partes viram widgets/features (fila, por mesa, ações do pedido). | 0011 |
