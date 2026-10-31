# 0017. CI e proteção de branches no GitHub

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
A spec 002 criou as três suítes (servidor, interface, ponta a ponta), mas deixou a execução automática fora de escopo.
O fluxo de branches do ADR 0018 tem regras (nome de branch, mensagem de commit, origem dos PRs) que precisam valer no
servidor, porque hooks locais podem ser pulados. O repositório é público no GitHub.

## Decisão
- **`.github/workflows/ci.yml`**, em `pull_request` e `push` para `dev` e `main` (e manual), com três jobs paralelos,
  os mesmos comandos de quem desenvolve:
  - `backend`: `dotnet test --solution backend/SoftwareHamgueria.slnx` (o Testcontainers usa o Docker do runner);
  - `frontend`: `npm ci`, `npm test`, `npm run lint`, `npm run build`;
  - `e2e`: Playwright no modo `mock`, com os traces publicados como artefato quando falha.
- **`.github/workflows/convencao.yml`** (job `convencao`), só em PR, também quando o PR é editado: aplica o ADR 0018
  chamando `scripts/git/convencao.sh`, o mesmo script dos hooks (ADR 0019). Separado para revalidar o título sem
  rodar as suítes de novo.
- Em PR, um push novo cancela o run anterior; em `dev`/`main`, todo commit integrado é verificado.
- **Rulesets:**
  - `main`: exige PR, exige os checks `backend`, `frontend`, `e2e` e `convencao` verdes, bloqueia force-push e
    exclusão; não exige aprovação.
  - `dev`: só bloqueia force-push e exclusão. Commit e push direto são permitidos (ADR 0018); o CI roda em cada push.
- Fora do CI: o e2e no modo `real` (precisa de API + RabbitMQ) e os detectores Visant (ainda têm achados da linha
  de base em "Candidatos a spec").
- Permissões mínimas (`contents: read`). Dados do PR (título, branches) entram nos scripts por `env`, nunca
  interpolados no comando.

## Alternativas consideradas
- **Um job só, em sequência:** mais simples, mas leva a soma dos tempos e esconde qual camada quebrou.
- **commitlint ou actions de terceiros para a convenção:** dependência Node e de terceiros para regras que cabem em
  expressões regulares, e não serviria aos hooks locais.
- **e2e real com `services:` (Postgres + RabbitMQ):** cobre mais, mas dobra tempo e manutenção.

## Consequências
- ✅ Todo PR mostra se algum critério quebrou, com o nome da regra no teste, e se segue a convenção.
- ✅ As regras do ADR 0018 valem mesmo para quem não instalou os hooks.
- ⚠️ Os nomes dos jobs fazem parte do ruleset: renomear um job exige atualizar o ruleset.
- ⚠️ As actions estão por tag de versão maior (`@v4`), não por SHA.
- ⚠️ O e2e baixa o Chromium e dependências a cada execução (alguns minutos).
