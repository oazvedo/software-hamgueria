# 012. Só o dono muda o pedido, e status não anda para trás: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Qualquer pessoa que veja o id de um pedido (na comanda, no acompanhamento) consegue cancelá-lo ou trocar os itens. Os
nomes das comandas aparecem para a mesa toda, então eles também não protegem nada. E a cozinha consegue "servir" um
pedido já cancelado ou devolver para produção um pedido já servido.

## Objetivo
Só quem fez o pedido, no aparelho em que o fez, ou a cozinha, pode alterá-lo ou cancelá-lo. Pedido servido ou cancelado
não muda mais.

## Regras de negócio
- RN-01: ao criar o pedido, o aparelho recebe uma chave secreta dele, uma única vez; o sistema não guarda a chave, só
  uma impressão dela.
- RN-02: alterar ou cancelar exige essa chave ou o login da cozinha. Pedidos antigos, sem chave, só a cozinha muda.
- RN-03: em produção ou rascunho, o pedido pode ir para servido ou cancelado, e voltar a rascunho quando alterado.
  Servido e cancelado são finais.
- RN-04: as recusas dizem o motivo em português.

## Critérios de aceite
- **CA-01:** Dado um pedido criado, então a resposta traz a chave do dono; consultar o pedido depois não a traz.
- **CA-02:** Dado o dono (com a chave) ou a cozinha, quando cancela ou altera, então dá certo.
- **CA-03 (falha):** Dado alguém sem a chave, ou com a chave de outro pedido, quando tenta cancelar ou alterar, então é
  negado ("Só quem fez o pedido (ou a cozinha) pode alterar ou cancelar.") e nada muda.
- **CA-04:** Dado um pedido em produção ou rascunho, então ele pode ser servido, cancelado ou voltar a rascunho.
- **CA-05 (falha):** Dado um pedido servido ou cancelado, quando alguém tenta cancelar, alterar ou mudar o status, então
  é recusado com o motivo, e o status fica.

## Impacto na linha de base
001/CA-12 (qualquer um cancela) deixa de valer: era o comportamento a corrigir.

## Fora de escopo
- Login de cliente. Recuperar a chave em outro aparelho (a cozinha resolve).
