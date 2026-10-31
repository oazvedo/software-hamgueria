# 021. Telas carregadas sob demanda e a cozinha em partes: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O navegador baixava todas as telas (inclusive a da cozinha) na primeira visita, e a tela da cozinha era um arquivo de
quase 400 linhas que misturava fila, mesas, barra e ações.

## Critérios de aceite
- **CA-01:** Dado o cliente abrindo o cardápio, então ele não baixa o código da cozinha (cada tela é um pedaço próprio).
- **CA-02:** Dada uma tela ainda carregando, então aparece um indicador de carregamento, sem tela em branco.
- **CA-03:** Dada a cozinha, então fila, por mesa e histórico funcionam como antes (mesmos testes de tela e e2e).
