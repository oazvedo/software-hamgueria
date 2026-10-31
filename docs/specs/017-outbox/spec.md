# 017. Pedido e aviso gravados juntos, e aviso que não se perde: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O aviso de cada pedido (para outros sistemas) ficava numa fila em memória: se o sistema reiniciasse com a fila de
mensagens fora, os avisos esperando se perdiam, e uma falha entre gravar o pedido e enfileirar o aviso perdia o aviso.

## Critérios de aceite
- **CA-01:** Dado um pedido criado, alterado ou cancelado, então o aviso fica gravado junto com ele.
- **CA-02 (falha):** Dado que gravar o pedido falha, então nenhum aviso fica gravado (nenhum aviso aponta para pedido
  que não existe).
- **CA-03:** Dado um aviso gravado e ainda não enviado (de antes de um reinício, por exemplo), quando a fila de
  mensagens está disponível, então ele é enviado e marcado como enviado.
- **CA-04:** Dados avisos enviados há mais de 7 dias, então a limpeza diária os apaga, e nunca apaga um não enviado.

## Impacto
Spec 005: o comportamento visível continua (pedidos funcionam com a fila fora, avisos saem quando ela volta, em ordem);
o limite de 1000 avisos em memória (005/CA-06) deixa de existir: não há mais limite nem perda.

## Fora de escopo
- Consumidor dos avisos. Desduplicação no consumidor (o id da mensagem permite).
