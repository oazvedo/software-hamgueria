# 0006. Repositório persiste a própria escrita (sem Unit of Work)

- **Status:** Proposto
- **Data:** 2026-10-31 (retroativo)

## Contexto
Os repositórios atuais (`DishRepository`, `OrderRepository`, `TableRepository`, `ComandaRepository`) chamam
`SaveChangesAsync` dentro de `AddAsync`, `UpdateAsync` e `UpsertAsync`. Não existe `IUnitOfWork`. As listagens
devolvem tudo (`GetAllAsync`), sem paginação.

## Decisão
- Cada método de escrita do repositório persiste na hora (`SaveChangesAsync` interno).
- O service faz no máximo **uma escrita de repositório por operação**. Se uma operação precisar gravar mais de um
  agregado de forma atômica, **pare e proponha um ADR** que introduza Unit of Work (não improvise uma transação).
- Leituras que alimentam DTOs com navegação usam `Include` no repositório (ex.: `Order` → `Table`, `Items` → `Dish`).
- Listagens sem paginação são aceitas enquanto o volume for de restaurante (dezenas de pratos e mesas).

## Alternativas consideradas
- **Repository + Unit of Work (padrão do repositório de estudo):** melhor para operações compostas; fica como
  candidato a spec, porque migrar mexe em todos os services.
- **Usar o `DbContext` direto no service:** quebraria a regra de dependência da Application (ADR 0004).

## Consequências
- ✅ Simples: cada chamada de repositório é autocontida.
- ⚠️ Operações compostas não são atômicas. Ex.: `OrderService.CreateAsync` grava o pedido e depois publica no RabbitMQ;
  se a publicação falhar, o pedido fica gravado sem evento.
- ⚠️ Sem paginação, `GET /api/order` cresce sem limite com o tempo.
