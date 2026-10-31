# 0014. Comanda identificada por mesa + nome

- **Status:** Proposto
- **Data:** 2026-10-31 (retroativo)

## Contexto
Uma mesa pode ter várias pessoas pagando separado, e a mesma pessoa faz vários pedidos ao longo da noite. Depois que
alguém fecha a conta, o mesmo nome pode voltar a ser usado na mesma mesa (outro cliente, outro dia).

## Decisão
- **Não existe entidade `Comanda`.** Uma comanda é o conjunto de pedidos com o mesmo `TableId` + `CustomerName`
  (comparação sem diferenciar maiúsculas; o nome é salvo com `Trim()`, até 60 caracteres).
- O fechamento é registrado em `ComandaClosure` (único por `TableId` + `CustomerName`), que guarda o `ClosedAt`
  do último fechamento. Fechar de novo só atualiza o `ClosedAt` (upsert).
- Um pedido pertence a uma comanda **fechada** se `order.CreatedAt <= closure.ClosedAt`. Um pedido novo com o mesmo
  nome reabre a comanda, porque é posterior ao fechamento. A flag `ComandaClosed` é calculada no `OrderService` e
  enviada no `OrderResponseDto`.
- Fechar a comanda é público (cliente ou caixa).

## Alternativas consideradas
- **Entidade `Comanda` com id e status:** modelo mais explícito, mas exige criar a comanda antes do primeiro pedido e
  migrar os pedidos existentes.
- **Apagar ou arquivar os pedidos ao fechar:** perderia o histórico da cozinha.

## Consequências
- ✅ Pedir é simples: o cliente só informa o nome no checkout.
- ⚠️ A regra de "fechada" vive em dois lugares (backend e mock MSW, ADR 0013).
- ⚠️ A comparação `ToLower()` no repositório não usa índice e depende da collation do Postgres.
- ⚠️ Qualquer pessoa que saiba a mesa e o nome pode ver ou fechar a comanda (ADR 0009).
