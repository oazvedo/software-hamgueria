# 0026. Fluxo autônomo: specs e ADRs sem paradas de aprovação

- **Status:** Aceito (decisão do usuário, 2026-10-31)
- **Data:** 2026-10-31

## Contexto
O ADR 0002 manda a IA parar para revisão no `spec.md`, no `plan.md` e no `tasks.md`, e só o usuário aceita ADRs. Nas
specs 004 a 006, cada parada virou um "aprovado" sem ajuste, e o usuário pediu para deixar de aprovar: "não quero mais
ficar aprovando as coisas", "você está completamente autônomo por fazer todas as mudanças". O registro (spec, plan,
tasks, ADR, testes) continua valioso; a espera, não.

## Decisão
Vale **por enquanto**, até o usuário pedir de volta as aprovações:
- A IA escreve `spec.md`, `plan.md` e `tasks.md` e implementa **na sequência, sem parar** entre as fases. Quando houver
  uma escolha, segue a opção recomendada e registra o porquê no `plan.md` (ou num ADR).
- ADR novo nasce `Aceito`, com a anotação "(fluxo autônomo, ADR 0026)". Mudar um ADR aceito continua exigindo um ADR
  novo que o substitua.
- Cada spec vai numa branch `feat/NNN-nome` com PR para a `dev`. Com o CI verde e a revisão do Claude (ADR 0020)
  respondida, a própria IA integra o PR (`gh pr merge --merge`).
- Continua exigindo o usuário: PR `dev` → `main`, deploy real, apagar dados fora do ambiente de teste, gastar dinheiro
  em serviço externo e qualquer ação irreversível fora do repositório.
- Ao fim de um lote, a IA entrega um relatório curto do que fez, do que decidiu sozinha e do que ficou no backlog.

## Alternativas consideradas
- **Manter as três paradas (ADR 0002):** segura, mas o usuário deixou claro que virou fricção sem ganho.
- **Abandonar specs e ADRs:** mais rápido, mas perde o histórico e os critérios de aceite que viram testes.

## Consequências
- ✅ Lotes grandes de trabalho andam sem esperar o usuário.
- ✅ O histórico continua: cada mudança tem spec, testes e PR.
- ⚠️ Uma decisão ruim só aparece no relatório ou no PR. Mitigação: decisões registradas por escrito e PRs pequenos,
  um por spec, para reverter fácil.
