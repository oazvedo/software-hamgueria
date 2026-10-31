# 0010. Eventos de pedido publicados no RabbitMQ

- **Status:** Substituído em parte por [0025](0025-eventos-de-pedido-com-fila-de-espera-em-memoria.md) (conexão e publicação)
- **Data:** 2026-10-31 (retroativo)

## Contexto
O requisito pede que criar, alterar e cancelar pedido seja propagado numa fila RabbitMQ. Inicialmente um consumer
também mudava o status dos pedidos a partir de mensagens externas; hoje o status é controlado à mão pela tela da cozinha.

## Decisão
- A Application depende só da porta `IOrderEventPublisher.Publish(routingKey, message)`; a Infrastructure implementa
  com `RabbitMqPublisher` (singleton), num exchange **topic** durável `hamgueria.orders`, mensagens persistentes.
- Routing keys: `order.created`, `order.updated`, `order.cancelled`. Corpo: o id do pedido em texto.
- Publique **depois** de persistir.
- O `OrderStatusConsumer` (fila `hamgueria.orders.status`, chave `order.status.#`, mensagem `<id>:<Status>`) existe mas
  está **desligado** no `InfrastructureModule`; o status vem do `PATCH /api/order/{id}/status` (Admin).

## Alternativas consideradas
- **Sem mensageria (só banco):** atenderia o app hoje, mas o requisito pede a fila e ela é parte do estudo.
- **Outbox pattern:** garante que evento e gravação andem juntos; fica para quando houver consumidor real (exige UoW, ADR 0006).

## Consequências
- ✅ Outros sistemas podem reagir a pedidos sem acoplar à API.
- ⚠️ Nenhum consumidor ativo hoje: os eventos são publicados e ficam sem leitor.
- ⚠️ Publicação síncrona (`GetAwaiter().GetResult()`) e conexão aberta no construtor: a API depende do RabbitMQ estar de pé para resolver o publisher.
- ⚠️ `CancelAsync` e `UpdateStatusAsync` não são simétricos: só o cancelamento publica.
