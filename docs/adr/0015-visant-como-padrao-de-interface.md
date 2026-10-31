# 0015. Sistema Visant como padrão de interface

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
O frontend tinha só regras de estrutura (ADR 0011) e skills de referência (`better-*`) sem um processo que dissesse
quando uma tela está pronta. Sem um portão, telas geradas por IA passam no build e no lint com tiques visíveis: seta
em texto, cor crua, `transition-all`, travessão e emoji na copy. A primeira varredura (2026-10-31) achou 6 cores cruas,
5 setas em texto, 1 `transition-all` e 11 tiques de copy.

## Decisão
- As skills `visant-killer`, `visant-frontend`, `visant-motion`, `visant-copy` e `visant-refine`
  ([pedrojaques99/visant-skills-public](https://github.com/pedrojaques99/visant-skills-public), MIT) são o padrão para
  decidir e auditar interface. Os `better-*` ficam como referência de valores exatos.
- Toda spec que muda tela roda, no fim do bloco de frontend, o `killer-scan` + `copy-lint` + `motion-lint` sobre os
  arquivos tocados, e no fim da spec o fluxo completo da `visant-killer` com relatório em `docs/audits/`.
- Toda tela nova é classificada antes de ser desenhada (A aquisição, B trabalho, C confiança) e cada elemento nomeia
  a variável que move (*cut test* da `visant-frontend`). Na prática: cardápio e checkout são A, a tela da cozinha é B,
  cancelar pedido e fechar comanda são C.
- O detector `impeccable` entra como devDependency do `frontend/` com versão exata (`4.8.0`), nunca via `npx` sem versão.
- As proibições da `visant-copy` (sem travessão, emoji, ponto-e-vírgula, "não é X, é Y") valem para toda copy nova
  ou reescrita. A copy existente só muda numa spec própria.
- Os motivos de identidade dev (`code-crumb`, `mono-everywhere`) não são removidos fora de uma spec que decida quais
  ficam, com exceção escrita na linha.
- Os demais ajustes para este repo (caminhos, tokens `oklch`, `lucide-react`) estão no `CLAUDE.md`, seção
  "Interface: Visant é o padrão".

## Alternativas consideradas
- **Só os `better-*`:** dão valores exatos, mas não têm portão nem detector; a revisão vira opinião.
- **`impeccable` como sistema completo:** 23 comandos, focado em gosto; a Visant já o usa como um dos detectores e
  acrescenta o *cut test* e o portão por relatório.
- **Os dois orquestradores (`visant-killer` e `better-interface`) juntos:** competem pelos mesmos gatilhos ("revisa
  essa tela"). O `better-interface` foi removido, e com ele o `interface-review`, que dependia dele.

## Consequências
- ✅ Critério objetivo de "tela pronta": detectores verdes + superfície classificada + tela aberta de verdade.
- ✅ Copy em pt-BR com linter próprio, que os `better-*` não têm.
- ⚠️ Os scripts são de terceiros e rodam no repo: toda atualização das skills exige revisar o diff dos `scripts/`.
- ⚠️ O detector de ruído marca a identidade dev do produto (`code-crumb`, `mono-everywhere`): até a spec que decide,
  o gate do frontend não fecha verde por causa desses achados.
- ⚠️ As proibições de copy mudam a voz atual em 11 pontos quando a spec de voz for feita.
- ⚠️ O `impeccable` executa um binário nativo (~22 MB) vindo do npm; a confiança está no checksum do pacote, não numa leitura do código.
- ⚠️ A verificação de tela aberta (`verify-screens`) depende do Playwright, que ainda não está no projeto.
