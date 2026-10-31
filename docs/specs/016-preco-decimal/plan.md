# 016. Plan

- **ADRs aplicados:** 0005, 0007, 0013, 0016, 0021. Sem ADR novo (tipo de campo, não decisão de arquitetura).

## Abordagem
`Dish.Price` e os DTOs passam de `double` para `decimal`; `HasPrecision(10, 2)`; migration `DishPriceAsDecimal`
(`ALTER COLUMN ... TYPE numeric(10,2)`, conversão no lugar). Validador: `PrecisionScale(10, 2)`. Seed com literais `m`.
Handler MSW com a mesma regra. O front segue com `number` (só exibe e soma o que a API manda).

## Verificação
CA-01: `DishPriceEndpointsTests` (JSON `"price":19.99`). CA-02: `DishPriceTests`, `DishPriceEndpointsTests`,
`validation.test.ts`. CA-03: `DishPriceTests`.
