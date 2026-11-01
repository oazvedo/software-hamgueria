# 026. Teste instável: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Na spec 011 ficou registrada uma falha rara da suíte funcional (1 em ~10 rodadas), com causa não capturada.

## Investigação
- Duas causas de instabilidade já tinham sido achadas e corrigidas: a porta fixa do broker de teste caía na faixa
  efêmera do Docker (`port is already allocated`, corrigido na 011) e o invariante do outbox contava linhas de outro
  teste no banco compartilhado (corrigido na 017).
- Depois disso: 6 rodadas seguidas da suíte funcional, locais, sem falha; e nenhuma falha nas últimas 40 execuções do
  CI.

## Critérios de aceite
- **CA-01:** A suíte funcional passa em rodadas repetidas (6 locais seguidas) e o CI não registra falha intermitente.

## Também neste PR
- Dependabot ignora versões maiores das imagens (Postgres, RabbitMQ...), que exigem migração de dados e são decisão
  manual. Os PRs maiores que ele já abriu ficam para o usuário.
- `npm install-scripts deny web-vitals` (o `prepare: husky` do pacote só serve ao desenvolvimento dele).
