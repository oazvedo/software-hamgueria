# 0031. Eventos de pedido por outbox transacional no Postgres

- **Status:** Aceito (fluxo autônomo, ADR 0026)
- **Data:** 2026-10-31

## Contexto
O ADR 0025 deixou o pedido funcionar com o RabbitMQ fora, mas com dois custos aceitos: o evento ficava em memória
(perdido se a API reiniciasse com a fila fora) e gravar o pedido e enfileirar o evento eram passos separados (uma falha
entre os dois perdia o evento). O candidato "Unit of Work" pedia a gravação atômica, e o lote 2 (spec 017) a entrega.

## Decisão
- **Tabela `OutboxMessages`** (`Id`, `RoutingKey`, `Payload`, `CreatedAt`, `SentAt`, `Attempts`, `LastError`), no mesmo
  banco. `IOrderEventPublisher.Publish` (porta inalterada) passa a só **adicionar** a linha ao `AppDbContext` da
  requisição, sem salvar (`OutboxOrderEventPublisher`, scoped).
- **O serviço publica antes de gravar:** o `SaveChanges` do repositório (ADR 0006 continua: o repositório persiste a
  própria escrita) salva o pedido e o evento na mesma transação. Substitui o "publique depois de persistir" do ADR 0010.
- **`RabbitMqEventDispatcher` lê o outbox:** lotes de 50 por ordem de criação com `FOR UPDATE SKIP LOCKED` (duas
  instâncias nunca mandam a mesma linha), publica com confirms, marca `SentAt`. Recusa definitiva (nack) marca a linha
  como falha e segue; queda de conexão deixa a linha pendente. O progresso de um lote é salvo mesmo se ele parar no meio.
- **Saúde e métricas:** `pendingEvents` é a contagem de linhas não enviadas, ao vivo.
- **Retenção:** job `outbox-retention` (Hangfire, ADR 0029) apaga, todo dia, linhas enviadas há mais de 7 dias.
- Substitui o ADR 0025 (fila em memória, limite de 1000, perda no reinício).

## Alternativas consideradas
- **Manter a fila em memória:** simples, mas perde eventos no reinício e não é atômica.
- **Unit of Work explícito em todo serviço:** muda o ADR 0006 para todas as escritas; o outbox só precisa que o evento
  entre no mesmo `SaveChanges` do agregado, o que a ordem das chamadas já garante.
- **Biblioteca pronta (MassTransit, CAP):** traz outbox, mas também um framework de mensageria inteiro para três eventos.

## Consequências
- ✅ Pedido e evento gravados juntos ou nenhum; evento sobrevive a reinício e a queda longa do broker.
- ✅ Várias instâncias da API podem enviar sem duplicar linha.
- ⚠️ Entrega continua pelo menos uma vez (um confirm perdido reenvia); o `MessageId` da mensagem é o id da linha, para
  um consumidor futuro descartar duplicata.
- ⚠️ Polling a cada 0,5 s quando o outbox está vazio (uma consulta indexada leve).
