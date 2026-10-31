# 015. O cliente altera e cancela o próprio pedido pela tela: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
A spec 012 deu ao dono do pedido o direito de alterar e cancelar, mas a tela não oferecia nenhum dos dois.

## Critérios de aceite
- **CA-01:** Dado o aparelho que fez o pedido, quando o cliente cancela, então a tela pede confirmação ("Não dá para
  desfazer") e, confirmado, o pedido fica cancelado.
- **CA-02:** Dado o aparelho que fez o pedido, quando o cliente altera as quantidades, então o pedido muda; zerar um item
  o tira do pedido.
- **CA-03 (falha):** Dado que todos os itens foram zerados, então não dá para salvar e a tela diz que o pedido precisa de
  pelo menos um item (para desistir, cancelar).
- **CA-04 (falha):** Dado outro aparelho (sem a chave do pedido), então os botões não aparecem.
- **CA-05 (falha):** Dado um pedido servido ou cancelado, então os botões não aparecem.

## Fora de escopo
- Acrescentar pratos novos a um pedido já feito (o caminho é um pedido novo na mesma comanda).
