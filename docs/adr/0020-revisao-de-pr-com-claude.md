# 0020. Revisão de PR e `@claude` com o Claude Code Action

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
O CI (ADR 0017) prova que testes e convenção passam, mas não lê o diff contra as regras que não são executáveis:
ADRs, `CLAUDE.md`, critérios de aceite sem teste, endpoint sem handler MSW. Com o fluxo rápido de implementação
(ADR 0002), as revisões pesadas locais (`code-review`) só rodam quando pedidas; uma revisão automática no PR cobre
esse buraco sem atrasar quem implementa.

## Decisão
- `.github/workflows/claude.yml` com `anthropics/claude-code-action@v1`, dois jobs:
  - `review`: em todo PR (não rascunho, não fork) para `dev`/`main`, o Claude lê `CLAUDE.md`, ADRs e a spec citada,
    revisa o diff e posta comentários inline + um resumo fixo (sticky) com veredito. Ferramentas restritas a leitura
    do repositório e `gh pr diff/view/comment`.
  - `responder`: um comentário com `@claude` em issue ou PR, de quem tem acesso de escrita, aciona o Claude para
    responder ou implementar; branches criadas por ele saem da `dev` com prefixo `chore/claude-` (válido no ADR 0018).
- Credencial no secret `CLAUDE_CODE_OAUTH_TOKEN` (gerado com `claude setup-token`; usa a assinatura Claude).
- A revisão do Claude **não** é check obrigatório do ruleset: é parecer, não portão.

## Alternativas consideradas
- **`ANTHROPIC_API_KEY`:** funciona igual, mas cobra por token à parte da assinatura.
- **Revisão obrigatória (check que bloqueia):** um falso positivo travaria o merge; o portão continua sendo o CI.
- **Só `@claude` sob demanda:** mais barato, mas a revisão some justamente quando ninguém lembra de pedir.

## Consequências
- ✅ Todo PR recebe uma leitura contra os ADRs, com arquivo:linha.
- ⚠️ Cada PR consome uso da assinatura; PRs grandes custam mais.
- ⚠️ Repositório público: PR de fork não roda a revisão (sem secrets), e só quem tem escrita aciona o `@claude`.
- ⚠️ O conteúdo do PR vira entrada do modelo; um PR malicioso poderia tentar manipular a revisão. Por isso as
  ferramentas da revisão são só de leitura e comentário.
