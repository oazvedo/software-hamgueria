# 022. Plan

- **ADRs aplicados:** 0011, 0015, 0017. Sem ADR novo.

## Abordagem
`scripts/ui/fsd-lint.mjs` (sem dependência) lê os imports `@/...` (estáticos e `import()`) de `frontend/src` e aplica as
três regras; entra no `gate.sh` como quinto detector; `fsd-lint.test.sh` cobre os casos e roda no CI. Correções: os
contratos da API (`Dish`, `DishCategory`, `Table`) vão para `shared/api/contracts.ts` (as entities reexportam), o
carrinho usa `OrderableDish` em vez de importar `entities/dish`, e os mocks importam do `shared`.

## Verificação
CA-01, CA-02: `fsd-lint.test.sh` (limpo, subindo, cruzado, profundo, shared importando entity, import dinâmico).
CA-03: `sh scripts/ui/gate.sh` aberto.
