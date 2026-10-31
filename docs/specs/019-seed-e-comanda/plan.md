# 019. Plan

- **ADRs aplicados:** 0005, 0013, 0022. Sem ADR novo.

## Abordagem
`DatabaseSeeder.Seed` roda numa transação com `pg_advisory_xact_lock` (chave fixa): a checagem e a inserção ficam
serializadas entre instâncias. `OrderService.CloseComandaAsync` confere a mesa e lança `NotFoundException`; o handler
MSW faz o mesmo.

## Verificação
CA-01: `SeedAndComandaTests` (4 seeds em paralelo num Postgres novo). CA-02: `SeedAndComandaTests`.
