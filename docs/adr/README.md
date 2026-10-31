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
| [0001](0001-registrar-decisoes-em-adrs.md) | Registrar decisões em ADRs | Proposto |
| [0002](0002-fluxo-spec-driven-com-ia.md) | Fluxo spec-driven com IA | Proposto |
| [0003](0003-monorepo.md) | Monorepo backend + frontend | Proposto |
| [0004](0004-backend-ddd-em-quatro-camadas.md) | Backend em DDD com quatro camadas | Proposto |
| [0005](0005-postgresql-ef-core-migrations.md) | PostgreSQL + EF Core, migrations e seed na inicialização | Proposto |
| [0006](0006-repositorio-persiste-a-propria-escrita.md) | Repositório persiste a própria escrita (sem Unit of Work) | Proposto |
| [0007](0007-dtos-record-com-mapeamento-manual.md) | DTOs `record` com mapeamento manual | Proposto |
| [0008](0008-api-rest-com-controllers.md) | API REST com Controllers | Proposto |
| [0009](0009-autenticacao-jwt-admin-unico.md) | Autenticação JWT com admin único | Proposto |
| [0010](0010-eventos-de-pedido-no-rabbitmq.md) | Eventos de pedido publicados no RabbitMQ | Proposto |
| [0011](0011-frontend-react-fsd-shadcn.md) | Frontend React com FSD, Atomic Design e shadcn/ui | Proposto |
| [0012](0012-estado-tanstack-query-e-zustand.md) | Estado: TanStack Query (servidor) e Zustand (cliente) | Proposto |
| [0013](0013-msw-mocks-por-padrao.md) | Mocks de API com MSW ligados por padrão | Proposto |
| [0014](0014-comanda-como-mesa-mais-nome.md) | Comanda identificada por mesa + nome | Proposto |
| [0015](0015-visant-como-padrao-de-interface.md) | Sistema Visant como padrão de interface | Aceito |
