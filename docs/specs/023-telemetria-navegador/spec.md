# 023. Ver o que acontece no navegador: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
A observabilidade da spec 010 parava na API: um erro de JavaScript na tela do cliente ou uma página lenta no celular
não deixavam rastro, e não dava para ligar uma tela lenta à requisição lenta no servidor.

## Critérios de aceite
- **CA-01:** Dada uma chamada da tela à API, então ela leva um identificador de trace (W3C `traceparent`) que a API
  continua, ligando a tela ao trace do servidor.
- **CA-02:** Dado um erro de JavaScript na tela, então ele chega aos logs da API com mensagem, pilha e página.
- **CA-03 (falha):** Dada uma página com erro em laço, então ela manda no máximo 5 erros.
- **CA-04:** Dadas as medidas de Web Vitals (LCP, CLS, INP, FCP, TTFB), então viram métrica, separadas por página sem
  ids (rota com `:param`).
- **CA-05 (falha):** Dada telemetria inválida (vazia, nome desconhecido, valor negativo), então é recusada sem eco.
- **CA-06 (falha):** Dado alguém mandando telemetria demais, então passa a receber 429 (60 por minuto por IP).

## Fora de escopo
- SDK de OpenTelemetry no navegador (spans do front). O `traceparent` já liga as duas pontas.
