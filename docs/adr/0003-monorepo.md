# 0003. Monorepo backend + frontend

- **Status:** Aceito
- **Data:** 2026-10-31 (retroativo)

## Contexto
Backend e frontend evoluem juntos: quase toda feature (comandas, tela da cozinha) mexe no contrato da API e na tela
ao mesmo tempo. É um projeto de portfólio de uma pessoa só.

## Decisão
Um único repositório com `backend/` (.NET, solução `SoftwareHamgueria.slnx` + `docker-compose.yml`) e `frontend/`
(Vite). A documentação de processo (`docs/`, `.claude/`, `CLAUDE.md`) fica na raiz. Uma spec pode tocar as duas pastas.

## Alternativas consideradas
- **Dois repositórios:** uma feature viraria dois PRs sincronizados, sem ganho para um time de uma pessoa.
- **Workspace com ferramenta de monorepo (Nx, Turborepo):** complexidade sem retorno com só duas aplicações em stacks diferentes.

## Consequências
- ✅ Uma spec, um branch, um PR, mesmo quando muda contrato + tela.
- ⚠️ Os comandos de build e lint são separados por pasta; não há um comando único de verificação.
