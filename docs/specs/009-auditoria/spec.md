# 009. Auditoria: quem mudou o quê, quando e de qual tela: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Quando um pedido some, muda de status ou um prato muda de preço, ninguém sabe quem fez, quando nem de onde. Não há
como investigar uma reclamação ("eu não cancelei esse pedido") nem um erro da cozinha.

## Objetivo
Toda mudança em pedidos, itens, pratos, mesas e fechamentos de comanda fica registrada pelo próprio banco, com antes e
depois, o nome de quem fez e a tela de onde saiu. A cozinha consulta esse histórico.

## Regras de negócio
- RN-01: criar, alterar e remover registros nessas cinco áreas grava uma linha de histórico com o antes e o depois.
- RN-02: o autor é o nome da comanda (cliente) ou o usuário da cozinha (logado); a origem é a página aberta no navegador.
- RN-03: numa alteração, o histórico lista só os campos que mudaram; uma alteração que só mexe no horário de
  atualização não gera linha.
- RN-04: só a cozinha logada consulta o histórico, filtrando por tipo de registro e por registro.
- RN-05: mudanças sem tela (preparação do banco, rotinas) ficam com o autor "sistema"; requisição sem nome, "anônimo".
- RN-06: o nome do cliente é declarado por ele; serve para investigar, não para dar permissão.

## Critérios de aceite
- **CA-01:** Dado um pedido enviado pela tela, quando a cozinha consulta o histórico do pedido, então vê a criação com
  os dados gravados.
- **CA-02:** Dado o cliente "João Ção" no cardápio, quando envia o pedido, então o histórico mostra "João Ção" como autor
  e a página do cardápio como origem.
- **CA-03:** Dada a cozinha logada, quando muda o status de um pedido, então o histórico mostra o usuário da cozinha e
  só o campo de status, com o valor antigo e o novo.
- **CA-04:** Dado um pedido alterado (itens trocados), quando a cozinha consulta, então vê a remoção do item antigo e a
  inclusão do novo.
- **CA-05 (falha):** Dado alguém sem login da cozinha, quando pede o histórico, então é negado.
- **CA-06 (falha):** Dado um filtro por um tipo de registro que não existe, quando a cozinha consulta, então é recusado
  com o motivo.
- **CA-07:** Dada a tela da cozinha, quando abre "Histórico", então vê as mudanças mais recentes, com quem, o quê, os
  campos alterados (de/para) e a tela de origem, e pode carregar mais.

## Fora de escopo
- Desfazer uma mudança pelo histórico. Retenção e limpeza (spec 011). Histórico para o cliente.
