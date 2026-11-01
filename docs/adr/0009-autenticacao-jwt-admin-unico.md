# 0009. Autenticação JWT com admin único

- **Status:** Substituído em parte por [0030](0030-dono-do-pedido-por-token.md) (quem altera e cancela pedido)
- **Data:** 2026-10-31 (retroativo)

## Contexto
O requisito separa dois públicos: o cliente anônimo (faz pedido, pede alteração, cancela, acompanha e fecha a comanda)
e o admin/cozinha (gerencia cardápio e mesas, vê todos os pedidos, muda status).

## Decisão
- `POST /api/auth/login` valida usuário e senha contra `Auth:AdminUsername`/`Auth:AdminPassword` da configuração e
  devolve um JWT HS256 (8 h) com o claim de papel `Admin`. Não há tabela de usuários.
- Endpoints restritos usam `[Authorize(Roles = "Admin")]`; o resto é público por omissão.
- No frontend, o token fica em `shared/auth/authToken` e o interceptor do `httpClient` anexa `Authorization: Bearer`.
- Ao criar um endpoint, decida explicitamente no `plan.md` se ele é **público** ou **Admin**.

## Alternativas consideradas
- **ASP.NET Identity com usuários no banco:** necessário para vários funcionários, excessivo para um portfólio com uma cozinha.
- **Autorização por permissão (`[HasPermission]`):** mais granular; vira ADR novo se surgirem papéis além de Admin.

## Consequências
- ✅ Simples e suficiente para separar cliente e cozinha.
- ⚠️ Credenciais e segredo JWT em `appsettings` com valores de dev (`admin`/`admin123`), e um fallback hardcoded no
  `AuthService`. Rode `security-review` em qualquer mudança aqui.
- ⚠️ Endpoints públicos de pedido não verificam quem é o dono da comanda: qualquer um com o id pode cancelar.
