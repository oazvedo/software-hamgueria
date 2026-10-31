# 025. Comanda esquecida aberta: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Uma comanda que o cliente esqueceu de fechar fica aberta para sempre, e ninguém percebe (a lista da cozinha só mostra
os finalizados das últimas 24 h, spec 020).

## Decisão do produto
A cozinha é **avisada**; nada fecha sozinho. Fechar não dá para desfazer (um pedido novo com o mesmo nome abre outra
comanda) e pode haver gente ainda na mesa. Decisão tomada no lote 2 como opção recomendada.

## Critérios de aceite
- **CA-01:** Dada uma comanda aberta sem pedido novo há mais de 6 horas (de qualquer idade, mesmo fora da janela de
  24 h), então ela aparece no aviso da cozinha, com mesa, nome, há quanto tempo e quantos pedidos.
- **CA-02:** Dada uma comanda com pedido recente ou já fechada, então ela não aparece.
- **CA-03 (falha):** Sem login da cozinha, a lista é negada; horas fora de 1 a 72 são recusadas com o motivo.
- **CA-04:** Dado o aviso, quando a cozinha toca "Fechar comanda", então ela fecha e sai do aviso.
