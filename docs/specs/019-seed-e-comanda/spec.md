# 019. Seed sem duplicar e comanda só de mesa que existe: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Duas instâncias da API subindo juntas viam o cardápio vazio ao mesmo tempo e semeavam duas vezes (achado da 002). E
fechar a comanda de uma mesa que não existe era aceito, criando um fechamento órfão (achado da 003).

## Critérios de aceite
- **CA-01:** Dadas várias instâncias semeando o mesmo banco vazio ao mesmo tempo, então o cardápio e as mesas existem
  uma vez só.
- **CA-02 (falha):** Dada uma mesa que não existe, quando alguém fecha uma comanda nela, então é recusado ("A mesa
  informada não existe.").
