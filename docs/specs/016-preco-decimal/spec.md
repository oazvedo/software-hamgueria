# 016. Preço exato, em centavos: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O preço dos pratos era guardado como número de ponto flutuante, que não representa centavos com exatidão
(0,10 + 0,20 não dá 0,30). Em dinheiro, isso vira centavo sumindo ou sobrando em total e relatório.

## Critérios de aceite
- **CA-01:** Dado um prato de R$ 19,99, quando é gravado e lido, então volta exatamente R$ 19,99.
- **CA-02 (falha):** Dado um preço com mais de 2 casas (ex.: 19,999), quando a cozinha cadastra, então é recusado com o
  motivo, em vez de arredondado em silêncio.
- **CA-03:** Dado somar preços, então nenhum centavo se perde.

## Fora de escopo
- Moedas além do real. Os preços já gravados são convertidos sem perda (eram valores de 2 casas).
