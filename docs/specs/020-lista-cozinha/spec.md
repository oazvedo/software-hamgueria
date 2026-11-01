# 020. A lista da cozinha não cresce para sempre: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
A tela da cozinha baixava todos os pedidos da história do restaurante a cada atualização: com o tempo, a tela e a API
ficariam lentas.

## Critérios de aceite
- **CA-01:** Dado um pedido em andamento de qualquer idade, então ele aparece na cozinha.
- **CA-02:** Dado um pedido servido ou cancelado há mais de 24 h, então ele não aparece mais; os das últimas 24 h
  aparecem, e a tela diz que os finalizados são das últimas 24 h.
- **CA-03:** A lista nunca passa de 500 pedidos (os mais recentes).

## Fora de escopo
- Busca de pedidos antigos (o histórico da auditoria e o banco guardam tudo).
