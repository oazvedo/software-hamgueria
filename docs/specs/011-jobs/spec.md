# 011. Tarefas de fundo com repetição e painel: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O histórico de auditoria cresce sem limite e não há onde rodar uma limpeza periódica, nem ver se uma rotina falhou e
tentar de novo.

## Objetivo
Rotinas de fundo rodam sozinhas no horário, tentam de novo quando falham e aparecem num painel da cozinha. A primeira
apaga o histórico de auditoria mais antigo que o prazo.

## Critérios de aceite
- **CA-01:** Dado o sistema no ar, então a limpeza do histórico está agendada para todo dia às 03:00 (UTC).
- **CA-02:** Dado histórico mais antigo que o prazo (180 dias por padrão, configurável), quando a limpeza roda, então
  ele é apagado e o recente fica.
- **CA-03 (falha):** Dado um prazo inválido ou menor que 7 dias na configuração, quando a limpeza roda, então usa os
  180 dias padrão.
- **CA-04:** Dado o painel de rotinas, quando alguém o abre, então precisa do usuário e da senha da cozinha; sem eles
  (ou errados), é negado.

## Fora de escopo
- Outras rotinas (ex.: fechar comandas esquecidas), que entram como specs próprias usando esta fila.
