# 0019. Git hooks versionados no repositório

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
O CI (ADR 0017) barra commits e branches fora do ADR 0018, mas só depois do push, quando corrigir exige reescrever
commits. Feedback no momento do commit é mais barato. O projeto roda no WSL.

## Decisão
- Hooks em `.githooks/`, ativados uma vez por clone: `git config core.hooksPath .githooks`.
- A lógica fica em `scripts/git/convencao.sh`, compartilhado com o CI; os hooks só chamam o script:

  | Hook | Regra |
  |---|---|
  | `pre-commit` | recusa commit na `main` e em branch com nome fora do padrão (a `dev` é permitida) |
  | `commit-msg` | recusa mensagem fora de Conventional Commits (aceita `Merge ...` e `Revert ...`) |
  | `pre-push` | recusa push para a `main` e de branch com nome fora do padrão (a `dev` é permitida) |

- POSIX `sh`, sem Node, Python nem framework de hooks. O script tem testes próprios
  (`sh scripts/git/convencao.test.sh`), que também rodam no CI.

## Alternativas consideradas
- **husky (Node) ou pre-commit (Python):** dependência de runtime; o husky ficaria preso ao `frontend/`.
- **Só CI, sem hooks:** o erro aparece depois do push.

## Consequências
- ✅ O erro aparece na hora do commit, com a mesma regra do CI.
- ⚠️ O `git clone` não instala hooks: quem esquece o `core.hooksPath` só descobre no CI.
- ⚠️ `--no-verify` pula os hooks; por isso o CI e o ruleset continuam valendo.
