# Architecture Decision Records

Um ADR registra **uma** decisão técnica: o contexto, o que foi decidido, as alternativas e as consequências.
Formato MADR enxuto ([template.md](template.md)). Para criar um novo, use a skill `adr`.

> **ADRs retroativos (0003 a 0014):** descrevem decisões que já estavam no código antes da adoção do fluxo
> spec-driven. Eles registram o estado atual, inclusive os custos dele. Mudar uma dessas decisões exige um ADR
> novo que a substitua.

## Regras
- Numeração sequencial de 4 dígitos. O número nunca é reutilizado.
- ADR aceito não se edita. Para mudar a decisão, crie um novo ADR e marque o antigo como `Substituído por NNNN`.
- Correções de texto (typo, link) podem ser feitas direto.
- Status: `Proposto` → `Aceito` ou `Rejeitado`. Depois, opcionalmente, `Substituído por NNNN`.
- Só o usuário aceita ou rejeita um ADR.

## Índice
| # | Título | Status |
|---|---|---|
| [0001](0001-registrar-decisoes-em-adrs.md) | Registrar decisões em ADRs | Aceito |
| [0002](0002-fluxo-spec-driven-com-ia.md) | Fluxo spec-driven com IA | Aceito |
| [0003](0003-monorepo.md) | Monorepo backend + frontend | Aceito |
| [0004](0004-backend-ddd-em-quatro-camadas.md) | Backend em DDD com quatro camadas | Aceito |
| [0005](0005-postgresql-ef-core-migrations.md) | PostgreSQL + EF Core, migrations e seed na inicialização | Aceito |
| [0006](0006-repositorio-persiste-a-propria-escrita.md) | Repositório persiste a própria escrita (sem Unit of Work) | Aceito |
| [0007](0007-dtos-record-com-mapeamento-manual.md) | DTOs `record` com mapeamento manual | Aceito |
| [0008](0008-api-rest-com-controllers.md) | API REST com Controllers | Aceito |
| [0009](0009-autenticacao-jwt-admin-unico.md) | Autenticação JWT com admin único | Aceito |
| [0010](0010-eventos-de-pedido-no-rabbitmq.md) | Eventos de pedido publicados no RabbitMQ | Aceito |
| [0011](0011-frontend-react-fsd-shadcn.md) | Frontend React com FSD, Atomic Design e shadcn/ui | Aceito |
| [0012](0012-estado-tanstack-query-e-zustand.md) | Estado: TanStack Query (servidor) e Zustand (cliente) | Aceito |
| [0013](0013-msw-mocks-por-padrao.md) | Mocks de API com MSW ligados por padrão | Aceito |
| [0014](0014-comanda-como-mesa-mais-nome.md) | Comanda identificada por mesa + nome | Aceito |
| [0015](0015-visant-como-padrao-de-interface.md) | Sistema Visant como padrão de interface | Aceito |
| [0016](0016-estrategia-de-testes.md) | Estratégia de testes | Aceito |
| [0017](0017-ci-com-github-actions.md) | CI e proteção de branches no GitHub | Aceito |
| [0018](0018-fluxo-de-branches-e-conventional-commits.md) | Fluxo de branches (main + dev) e Conventional Commits | Aceito |
| [0019](0019-git-hooks-versionados.md) | Git hooks versionados no repositório | Aceito |
| [0020](0020-revisao-de-pr-com-claude.md) | Revisão de PR e @claude com o Claude Code Action | Aceito |
| [0021](0021-validacao-de-entrada-com-fluentvalidation.md) | Validação de entrada com FluentValidation na Application | Aceito |
| [0022](0022-erros-em-problemdetails-com-handler-global.md) | Erros da API em ProblemDetails com handler global | Aceito |
| [0023](0023-deploy-em-ec2-com-docker-compose-e-caddy.md) | Deploy em uma instância EC2 com Docker Compose e Caddy | Aceito |
| [0024](0024-segredos-por-variaveis-de-ambiente-validados-no-startup.md) | Segredos por variáveis de ambiente, validados na inicialização | Aceito |
| [0025](0025-eventos-de-pedido-com-fila-de-espera-em-memoria.md) | Eventos de pedido com fila de espera em memória e envio em segundo plano | Aceito |
| [0026](0026-fluxo-autonomo-sem-aprovacoes.md) | Fluxo autônomo: specs e ADRs sem paradas de aprovação | Aceito |
| [0027](0027-auditoria-por-triggers-no-postgres.md) | Auditoria do banco por triggers no PostgreSQL, com autor e origem | Aceito |
