# Audit: frontend/src/features/submit-order

## Surfaces

One row per screen. **Fill this in before fixing anything.** Classifying
after changing things is justifying what was already done.

- **A acquisition**: whoever is not a customer yet. Persuade, prove, convert.
- **B work**: whoever uses it every day. Speed, density, keyboard.
- **C trust**: money, permission, destructive action. It cannot lie.

Applying A instincts to a B screen is the most expensive Visant frontend mistake.

| File | Surface (A/B/C) | Business variable it moves | Notes |
|---|---|---|---|
| src/features/submit-order/ui/CheckoutDialog.tsx | B | Pedidos enviados depois de uma recusa (spec 003, CA-11) | Mensagem do nome abaixo do input; erros de item num bloco `role="alert"`, só quando existem |

## Screen seen

One row per screen OPENED, with what was seen. Without this the gate does not
open: a frozen AnimatePresence passes tsc, ESLint, motion-lint, impeccable,
audit:design and copy-lint without lighting anything up.

Each row carries: route, width, data state, and what happened.
Before measuring anything, check that the page hydrated (see
`visant-killer/references/environment.md`): half the findings of one session
were the dev server lying, not the screen.

- `/menu` (Playwright, Chromium, 1280px, modo mock, carrinho com 1 prato): stepper desabilitado em 20, pedido no limite
  enviado e aberto na página da comanda (`e2e/cliente.spec.ts`, 003/CA-03). Recusa por campo vista nos testes de tela
  (`CheckoutDialog.test.tsx`), porque a própria tela impede chegar ao servidor com nome inválido (`maxLength`, botão
  desabilitado com nome vazio).

## Findings

Gates rodados em 2026-10-31: `killer-scan` (3 de 4) e `copy-lint` à parte em `features/submit-order`,
`widgets/order-json` e `shared/ui/molecules/QuantityStepper.tsx`. Copy nova da 003: zero achado.

Achados todos **anteriores à 003**, já listados em `docs/specs/README.md` → "Candidatos a spec", e não corrigidos aqui
(ajustes 8 e 9 do `CLAUDE.md`):
- `raw-hex`: semáforos `#ff5f56`, `#ffbd2e`, `#27c93f` em `CheckoutDialog.tsx:105-107` e `OrderJsonPanel.tsx:28-30`
  (candidato "Limpeza da linha de base Visant"; trocar por tokens exige tokens novos em `index.css`).
- `emoji`: `➕ nova comanda` em `CheckoutDialog.tsx:141` (candidato "Copy existente nas regras da `visant-copy`").
- `code-crumb`: `// selecione uma mesa para enviar o pedido` em `OrderJsonPanel.tsx:103` (candidato "Identidade dev vs.
  detector de ruído").
