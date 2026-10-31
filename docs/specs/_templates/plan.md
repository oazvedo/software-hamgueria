# NNN. Nome da feature: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0004, 0005, ...
- **ADRs novos propostos:** nenhum | NNNN

## Abordagem
Resumo técnico em 3 a 6 frases.

## Modelo de dados
| Entidade | Campo | Tipo | Regras |
|---|---|---|---|

Migration: `<NomeDaMigration>` (sim/não; destrutiva?).

## Contrato da API
| Verbo | Rota | Acesso | Request | Sucesso | Erros |
|---|---|---|---|---|---|
| POST | `/api/xxx` | Público / Admin | `CreateXxxDto` | 201 + `XxxResponseDto` | 400, 404 |

Eventos RabbitMQ: nenhum | `xxx.created` ...

## Backend: arquivos
Lista de arquivos novos e alterados, seguindo a skill `dotnet-feature`.

## Frontend: arquivos
Slices novas e alteradas (entities, features, widgets, pages), handler MSW e chaves de query invalidadas,
seguindo a skill `react-feature`.

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | Teste funcional / Manual | `Create_ValidRequest_Returns201` / passo a passo |

## Riscos e decisões pendentes
- ...
