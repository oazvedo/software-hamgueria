# 0016. Estratégia de testes

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
O projeto não tem nenhum teste (spec 002). A verificação de uma task hoje é só build + lint. Precisamos de três níveis
(regras do servidor, interface e ponta a ponta), rodando localmente no WSL com Docker, sem tocar no banco de
desenvolvimento e sem exigir RabbitMQ.

## Decisão
**Backend** (`backend/tests/`, incluídos no `SoftwareHamgueria.slnx`):
- `SoftwareHamgueria.UnitTests`: **xUnit v3** + **NSubstitute**. Domínio e services com repositórios falsos.
- `SoftwareHamgueria.FunctionalTests`: **WebApplicationFactory** + **Testcontainers (PostgreSQL)**. A API sobe de
  verdade (migrations + seed) num Postgres descartável, um container por execução. O `IOrderEventPublisher` é trocado
  por um falso em memória (sem RabbitMQ). Testes isolam dados por nome de comanda único, sem limpar o banco.
- Asserções com o `Assert` do xUnit (sem biblioteca extra).
- O `Program.cs` ganha `public partial class Program;` para a factory enxergá-lo.

**Frontend** (`frontend/`):
- **Vitest** + **jsdom** + **Testing Library**, arquivos `*.test.ts(x)` ao lado do código, na própria slice.
- As chamadas de API são atendidas pelos **mesmos handlers MSW** do app (`msw/node`), com `localStorage` limpo entre
  testes. Utilitários de teste em `shared/test/` (`renderWithProviders` com QueryClient + MemoryRouter).
- **Playwright** em `frontend/e2e/`, dois projetos: `mock` (padrão, sobe o Vite com MSW) e `real` (Vite sem mocks
  contra a API, pulado se a API não responder; a API roda apontando para a base `hamgueria_e2e`).

**Convenções:**
- Nome do teste cita o critério e a regra, em português:
  `[Fact(DisplayName = "001/CA-04: pedido sem nome de comanda é recusado")]`, `it('001/CA-02: ...')`. Método C# em
  inglês, `Metodo_Cenario_Resultado`.
- Todo critério de aceite tem um cenário de sucesso e, se houver, um de falha.
- Comandos: `dotnet test --solution backend/SoftwareHamgueria.slnx`, `cd frontend && npm test`, `npm run e2e`,
  `npm run e2e:real`.

## Alternativas consideradas
- **EF Core InMemory ou SQLite nos testes do servidor:** rápidos, mas não reproduzem o Postgres (índice único,
  `ToLower`, collation), justamente onde a regra de comanda mora.
- **Banco de desenvolvimento com limpeza entre testes:** viola o isolamento dos dados de dev e a repetibilidade.
- **Jest no frontend:** exige configuração extra para ESM e Vite; o Vitest reaproveita o `vite.config`.
- **Cypress no e2e:** mais pesado e sem suporte tão bom a múltiplos projetos/navegadores quanto o Playwright, que
  também é o que o `verify-screens` da Visant usa (ADR 0015).

## Consequências
- ✅ Cada critério da 001 vira um teste com nome rastreável; quebrar a regra deixa o teste vermelho com o nome dela.
- ✅ Os handlers MSW passam a ser testados junto com a interface, o que reduz a divergência mock × API (ADR 0013).
- ⚠️ Os testes do servidor exigem Docker rodando; o primeiro run baixa a imagem do Postgres.
- ⚠️ O Playwright exige baixar o Chromium e, no WSL, instalar dependências do sistema com `sudo`.
- ⚠️ O e2e no modo real cria dados na base `hamgueria_e2e` (separada da de dev), que acumula entre execuções.
