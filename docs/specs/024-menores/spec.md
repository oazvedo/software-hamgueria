# 024. Itens pequenos dos backlogs: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## O que resolve
- **Imagens por digest:** as imagens base (Dockerfiles e composes) mudavam por baixo a cada `pull`; agora têm digest fixo,
  e o Dependabot propõe as atualizações (Docker, compose, NuGet, npm e Actions) toda semana, para o CI julgar.
- **Cardápio público só com ativos:** `GET /api/dish?active=true` devolve só os pratos que a cozinha faz; a cozinha
  continua vendo todos sem o filtro.
- **Ids gerados no domínio:** `Order`, `Dish`, `Table` e `ComandaClosure` marcados como `ValueGeneratedNever`, como o
  `OrderItem` (spec 005), para não repetir o bug do `UPDATE` no lugar de `INSERT`.
- **Scripts de instalação do npm:** `msw` aprovado (mantém o `mockServiceWorker.js`); `web-vitals` negado na spec 023.

## Critérios de aceite
- **CA-01:** Dado um prato desativado, quando o cardápio público pede os pratos, então ele não vem; sem o filtro, vem.
- **CA-02:** As imagens das duas aplicações constroem com os digests fixados.
- **CA-03:** A suíte passa com os ids marcados como gerados no domínio (migration sem SQL).
