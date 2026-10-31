# 0018. Fluxo de branches (`main` + `dev`) e Conventional Commits

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
Até aqui os commits foram direto na `dev` (e antes na `main`). Isso não liga mudanças às specs, não dá um ponto onde o
CI (ADR 0017) barre uma quebra antes da integração, e mistura trabalho em andamento com código estável. Modelo
adaptado de outro projeto que segue a mesma metodologia (`acarolinelais/tcm`, ADR 0027 de lá).

## Decisão
**Branches de longa duração:** `main` (estável; só recebe PR da `dev` ou de `hotfix/*`) e `dev` (integração; recebe PR
das branches de trabalho e o PR de sincronização `main → dev`). Nenhuma aceita commit ou push direto.

**Branches de trabalho** saem da `dev` e voltam por PR:
```
<tipo>/<descricao-em-kebab-case>
feat/003-validacao-problemdetails   # spec: feat/ exige o número NNN e a pasta docs/specs/NNN-*
fix/comanda-sem-nome
ci/github-actions
```
Tipos: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `style`, `perf`, `ci`, `build`. Descrição só com `a-z`,
`0-9`, `.` e `-`. `hotfix/*` sai da `main`, volta para a `main`, e logo depois um PR `main → dev` sincroniza.

**Commits e títulos de PR** em Conventional Commits: `tipo(escopo opcional)!?: descrição`
(`feat(comanda): fecha comanda pelo caixa`). `Merge ...` e `Revert ...` gerados pelo git são aceitos.

**Pull requests:** uma spec = uma branch `feat/NNN-...` = um PR para a `dev`, que cita `docs/specs/NNN-nome`
(template em `.github/pull_request_template.md`). Integração por **merge commit**; a branch de trabalho é apagada
depois. Só integra com os checks verdes (ADR 0017); aprovação humana é opcional. A branch padrão do GitHub passa a ser
a `dev`.

## Alternativas consideradas
- **GitHub flow (só `main`):** mais simples, mas não separa integrado de estável.
- **GitFlow completo (`release/*`, tags):** sem deploy nem versões, seria só cerimônia.
- **Squash merge:** histórico linear, mas apaga os commits por fase, que são material de estudo.

## Consequências
- ✅ A `main` só recebe código que passou pela `dev` com o CI verde; cada PR de feature aponta para a sua spec.
- ⚠️ Toda mudança precisa de dois PRs para chegar à `main`.
- ⚠️ Os commits anteriores a este ADR não seguem todos a convenção (`add readme file`); a regra vale para os novos.
