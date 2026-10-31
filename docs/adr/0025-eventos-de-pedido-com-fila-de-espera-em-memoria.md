# 0025. Eventos de pedido com fila de espera em memória e envio em segundo plano

- **Status:** Substituído por [0031](0031-outbox-transacional.md)
- **Data:** 2026-10-31

## Contexto
O `RabbitMqPublisher` (ADR 0010) abre a conexão no construtor e publica de forma síncrona. Como ele é dependência do
`OrderService`, qualquer rota de pedido, inclusive as de leitura, devolve 500 se o RabbitMQ estiver fora; e um pedido
gravado com a fila fora responde 500 e perde o evento. O `smoke.sh` da spec 004 reproduziu isso depois de um
`docker compose restart`. A spec 005 pede que pedidos funcionem com a fila fora, com os eventos enviados depois, na
ordem, aceitando perda se a própria API reiniciar. Substitui em parte o ADR 0010 (conexão e publicação); exchange,
routing keys, corpo e "publique depois de persistir" continuam valendo.

## Decisão
- **A porta não muda:** a Application continua chamando `IOrderEventPublisher.Publish(routingKey, message)` depois de
  persistir. A implementação passa a só **enfileirar**, sem I/O e sem lançar por causa da fila.
- **Fila de espera na Application** (`Application/Messaging/PendingOrderEvents`, singleton, sem dependência externa):
  FIFO com limite de 1000. Cheia, descarta o mais antigo e registra o descarte (routing key e id do pedido) no log.
  `Publish` é implementado por `QueuedOrderEventPublisher`, que só chama `Enqueue`.
- **Envio em segundo plano na Infrastructure** (`RabbitMqEventDispatcher : BackgroundService`): conecta sob demanda,
  com nova tentativa a cada 5 s enquanto falhar, e usa *publisher confirms* com timeout de 5 s. Lê o primeiro evento
  (`TryPeek`), publica e só depois de confirmado o remove (`RemoveHead`). Se falhar, descarta a conexão, espera e
  tenta de novo o mesmo evento. Entrega **pelo menos uma vez** (um confirm perdido pode duplicar).
- **Ao desligar**, o dispatcher registra no log quantos eventos ficaram sem envio.
- **Estado para a saúde:** o dispatcher expõe `IsConnected`; um `IHealthCheck` da fila usa isso e o tamanho da fila de
  espera. `/api/health` filtra só o banco (tag `db`); `/api/health/details` mostra todos.
- Nenhum serviço de requisição depende de conexão com o RabbitMQ: resolver o `OrderService` nunca abre socket.

```csharp
// OrderService (igual a hoje)
await orderRepository.AddAsync(order);
publisher.Publish("order.created", order.Id.ToString());   // só enfileira; nunca lança por causa da fila
```

## Alternativas consideradas
- **Outbox no banco:** nenhum evento se perde, inclusive com a API reiniciando, mas exige gravar pedido e evento na
  mesma transação (Unit of Work, muda o ADR 0006). Fica para uma spec própria; esta decisão não a impede, porque a
  porta continua a mesma.
- **Publicar com retry dentro da requisição (Polly):** a requisição do cliente esperaria a fila voltar ou falharia no
  fim do mesmo jeito. Não atende "o cliente não vê erro".
- **Descartar o evento com log quando a fila está fora:** mais simples, mas perde todo evento de uma queda curta, que é
  o caso mais comum (reinício).
- **`System.Threading.Channels` como fila de espera:** o `DropOldest` descarta sem avisar qual item, e sem remover pela
  cabeça confirmada um descarte no meio do envio apaga o evento errado. Uma fila própria com lock e `RemoveHead(item)`
  é curta e deixa a regra explícita.

## Consequências
- ✅ Leitura e escrita de pedidos funcionam com o RabbitMQ fora, e a API reconecta sozinha (reinício em qualquer ordem).
- ✅ A Application continua sem saber de RabbitMQ; a fila de espera é testável em unidade.
- ⚠️ Eventos em memória se perdem se a API reiniciar com a fila fora (logado). Garantia total só com outbox.
- ⚠️ Entrega pelo menos uma vez: um consumidor futuro precisa tolerar duplicata (o corpo é o id do pedido).
- ⚠️ A ordem é global e serial: um evento que o broker recusa para sempre trava os seguintes. Mitigado: a recusa
  (`nack`) é logada e o evento é descartado; só falha de conexão ou timeout segura a fila.
