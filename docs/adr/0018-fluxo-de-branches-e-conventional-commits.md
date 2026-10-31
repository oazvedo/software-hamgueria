# 0018. Fluxo de branches (`main` + `dev`) e Conventional Commits

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
Até aqui os commits foram direto na `dev` (e antes na `main`). Isso não liga mudanças às specs, não dá um ponto onde o
CI (ADR 0017) barre uma quebra antes da integração, e mistura trabalho em andamento com código estável. Modelo
adaptado de outro projeto que segue a mesma metodologia (`acarolinelais/tcm`, ADR 0027 de lá).

## Decisão
**Branches de longa duração:**
- `main` (estável): só recebe PR da `dev` ou de `hotfix/*`. Nenhum commit ou push direto.
- `dev` (integração): aceita **commit e push direto** (ajuste de 2026-10-31: projeto de uma pessoa, o PR para a `dev`
  era cerimônia). Branch de trabalho + PR para a `dev` continua disponível e é o caminho para uma spec grande ou para
  pedir a revisão do Claude (ADR 0020). A `dev` não aceita force-push nem pode ser apagada.

**Branches de trabalho** (opcionais) saem da `dev` e voltam por PR:
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

**Pull requests:** quando uma spec usa branch, é `feat/NNN-...` com um PR para a `dev` que cita `docs/specs/NNN-nome`
(template em `.github/pull_request_template.md`). Integração por **merge commit**; a branch de trabalho é apagada
depois. Só integra com os checks verdes (ADR 0017); aprovação humana é opcional. A branch padrão do GitHub passa a ser
a `dev`.

## Alternativas consideradas
- **GitHub flow (só `main`):** mais simples, mas não separa integrado de estável.
- **GitFlow completo (`release/*`, tags):** sem deploy nem versões, seria só cerimônia.
- **Squash merge:** histórico linear, mas apaga os commits por fase, que são material de estudo.

## Consequências
- ✅ A `main` só recebe código que passou pela `dev` com o CI verde.
- ✅ Trabalho do dia a dia vai direto para a `dev`, sem PR.
- ⚠️ Um commit direto na `dev` só é verificado pelo CI depois do push: uma quebra pode ficar na `dev` até ser
  corrigida, mas nunca chega à `main`.
- ⚠️ Os commits anteriores a este ADR não seguem todos a convenção (`add readme file`); a regra vale para os novos.
