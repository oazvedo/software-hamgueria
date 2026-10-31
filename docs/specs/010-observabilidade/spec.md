# 010. Ver o que acontece dentro do sistema: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Quando algo fica lento ou falha em produção, só sobra o log do container. Não dá para ver o caminho de uma requisição,
quais consultas ela fez, quantos pedidos entram por hora nem quantos avisos estão esperando a fila.

## Objetivo
O mantenedor abre um painel e vê, para cada requisição, o caminho até o banco; vê números do restaurante (pedidos
criados, avisos enviados, descartados e esperando) e procura nos logs com filtros.

## Critérios de aceite
- **CA-01:** Dada uma requisição ao cardápio, quando o mantenedor abre o painel, então vê o trace dela com a consulta ao
  banco como parte do mesmo trace.
- **CA-02:** Dado um pedido criado, então o número de pedidos criados aumenta.
- **CA-03:** Dados avisos descartados por fila cheia, então o descarte é contado com o motivo, e o número de avisos
  esperando é visível a qualquer momento.
- **CA-04:** Dado o ambiente de desenvolvimento, quando o dev sobe os serviços, então o painel está em
  `localhost:18888` e a API manda telemetria para ele.
- **CA-05 (falha):** Dado o ambiente sem coletor configurado (testes, produção sem o perfil), então a API funciona
  normalmente, sem tentar enviar.
- **CA-06:** Dado o ambiente de produção, então os logs do console saem em JSON, e o painel (opcional) só é acessível de
  dentro da instância.

## Fora de escopo
- Alertas, retenção longa de telemetria, telemetria do navegador.
