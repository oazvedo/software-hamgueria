# 007. Todas as comandas da mesa num lugar só: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Numa mesa com várias comandas (cada pessoa pagando a sua), o cliente só vê a própria pelo atalho do topo, e o atalho só
aparece depois que ele digita o nome. As outras comandas ficam escondidas numa tela "da mesa" que mistura todos os
pedidos de todo mundo, longa e difícil de ler.

## Objetivo
Escolhida a mesa, um atalho "comandas" lista todas as comandas dela, abertas e fechadas, com o essencial de cada uma, e
leva ao detalhe de qualquer uma.

## Regras de negócio
- RN-01: o atalho aparece assim que a mesa é escolhida, mesmo sem nome de comanda.
- RN-02: a lista mostra cada comanda com nome, estado (aberta ou fechada), número de pedidos, quantos estão em produção
  e o total. As abertas vêm primeiro, das mais recentes para as mais antigas.
- RN-03: a comanda do próprio cliente (mesma mesa e mesmo nome) aparece marcada.
- RN-04: cada comanda leva ao seu detalhe. O endereço antigo "da mesa" leva para a lista.
- RN-05: o topo da lista mostra o total da mesa e quantas comandas estão abertas.

## Critérios de aceite
- **CA-01:** Dada uma mesa escolhida e nenhum nome, quando o cliente olha o topo, então vê o atalho de comandas.
- **CA-02:** Dada uma mesa com duas comandas abertas e uma fechada, quando o cliente abre a lista, então vê as três,
  com as abertas primeiro, cada uma com estado, pedidos, em produção e total, e o total da mesa.
- **CA-03:** Dado o cliente com a comanda "Ana" na mesa, quando abre a lista, então a comanda "Ana" aparece marcada
  como a dele.
- **CA-04:** Dada a lista, quando o cliente escolhe uma comanda, então abre o detalhe dela.
- **CA-05 (falha):** Dada uma mesa sem pedidos, quando o cliente abre a lista, então vê o aviso de que não há
  comandas e o caminho para o cardápio.
- **CA-06:** Dado o endereço antigo da mesa, quando é aberto, então leva à lista de comandas.

## Fora de escopo
- Pagar ou dividir a conta. Juntar comandas.
