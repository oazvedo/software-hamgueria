# 006. Telas limpas, com a identidade dev só onde ela ajuda: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, a spec inteira de uma vez.
> **Pré-requisito:** `npm install` no `frontend/` (detectores e Playwright). Sempre `export npm_config_yes=false`.

## Fase 1: base
- [x] **T01. Gate único da interface e o teste dele**
  - Arquivos: `scripts/ui/gate.sh`, `scripts/ui/gate.test.sh`
  - Cobre: CA-03 (amostra suja reprovada por cor, ruído, copy e motion; amostra limpa aprovada)
  - Verificação: `sh scripts/ui/gate.test.sh` → todos os casos `ok` (o gate em `frontend/src` ainda falha: é a linha de base)
  - Nota: os códigos de saída dos detectores não servem de veredito (o `killer-scan` sai 1 limpo, o `motion-lint` sai 0
    com achado MEDIUM); o `gate.sh` lê a saída. Ele roda de dentro de `frontend/` para o `npx` achar o `impeccable` local.

- [x] **T02. Idioma, tokens de janela, átomos e botão**
  - Arquivos: `frontend/index.html` (`lang="pt-BR"`), `app/styles/index.css` (`--window-*`), `shared/ui/atoms/WindowDots.tsx`,
    `shared/ui/atoms/DataText.tsx`, `shared/ui/atoms/index.ts`, `shared/ui/shadcn/button.tsx` (sem `transition-all`),
    `e2e/cliente.spec.ts` (CA-01)
  - Cobre: CA-01, RN-07, RN-08
  - Verificação: `node .claude/skills/visant-motion/scripts/motion-lint.mjs frontend/src` → 0 achados; `cd frontend && npm run build` → ok

## Fase 2: telas
- [x] **T03. Painel do pedido e envio: bolinhas, instrução e "Nova comanda"**
  - Arquivos: `widgets/order-json/ui/OrderJsonPanel.tsx` (+ teste), `features/submit-order/ui/CheckoutDialog.tsx` (+ teste)
  - Cobre: CA-04 (sem mesa), CA-08 (opção sem emoji), CA-10
  - Verificação: `cd frontend && npm test -- OrderJsonPanel CheckoutDialog` → verde

- [x] **T04. Cartão do pedido, acompanhamento e comanda**
  - Arquivos: `entities/order/ui/OrderCard.tsx` (+ `OrderCard.test.tsx` novo), `entities/order/model/orderStatus.ts`
    (+ teste), `pages/order-status/ui/OrderStatusPage.tsx` (+ teste), `pages/comanda/ui/ComandaPage.tsx`
    (+ `ComandaPage.test.tsx` novo)
  - Cobre: CA-04 (comanda fechada), CA-05, CA-06, CA-08 (servido)
  - Extra: `entities/order/ui/ItemNote.tsx` (novo), o marcador de observação usado nas 4 telas, no lugar do `↳` repetido.
  - Verificação: `cd frontend && npm test -- OrderCard orderStatus OrderStatusPage ComandaPage` → verde

- [x] **T05. Tela da cozinha: mono só em dado, ícones e copy**
  - Arquivos: `pages/production/ui/ProductionPage.tsx` (+ `ProductionPage.test.tsx` novo)
  - Cobre: CA-05, CA-07, CA-08 (fila vazia), CA-09
  - Verificação: `cd frontend && npm test -- ProductionPage` → verde

- [x] **T06. Exceções de identidade, categorias e bebidas**
  - Arquivos: `pages/menu/ui/MenuPage.tsx`, `features/auth/ui/LoginForm.tsx`, `widgets/header/ui/Header.tsx`,
    `pages/order-status/ui/OrderStatusPage.tsx`, `pages/production/ui/ProductionPage.tsx` (exceções `code-crumb`),
    `entities/dish/model/categories.ts`, `shared/mocks/data.ts`, `backend/.../Data/DatabaseSeeder.cs`
  - Cobre: CA-02, RN-02, RN-06
  - Extra: `e2e/support.ts` procura a opção "Nova comanda" sem diferenciar maiúscula.
  - Verificação: `sh scripts/ui/gate.sh` → sai 0; `grep -n "—" backend/src/SoftwareHamgueria.Infrastructure/Data/DatabaseSeeder.cs` → vazio

## Fase 3: fechamento
- [x] **T07. Auditoria visant-killer com as telas abertas**
  - Arquivos: `docs/audits/2026-10-31-frontend.md` (`--init-report`, superfícies A/B/C, telas vistas em claro e escuro, `--report`)
  - Cobre: CA-02, CA-10 (temas), CA-11
  - Achados da auditoria corrigidos no bloco: badge de status quebrando em 2 linhas a 390px (`ProductionPage`) e
    travessão no `<title>` do `index.html` (fora do alcance do `copy-lint`). O app só tem tema escuro (`class="dark"`
    fixo): CA-10 conferido no escuro, e os tokens das bolinhas são iguais nos dois temas por decisão.
  - Verificação: `node .claude/skills/visant-killer/scripts/killer-scan.mjs --report docs/audits/2026-10-31-frontend.md frontend/src` → gate aberto;
    `cd frontend && npm test && npm run lint && npm run e2e` → verde

- [x] **T08. Documentação e candidatos**
  - Arquivos: `CLAUDE.md` (comando do gate; ajuste 9 cita a spec 006), `docs/specs/README.md` (saem 4 candidatos, roadmap)
  - Cobre: impacto na linha de base
  - Verificação: `grep -n "scripts/ui/gate.sh" CLAUDE.md` → presente; `grep -n "Limpeza da linha de base\|lang=\"pt-BR\"\` no\|Copy existente\|Identidade dev" docs/specs/README.md` → vazio

## Backlog
<!-- Achados da simplify e da revisão que ficam fora do escopo desta spec. Um item por linha, com o motivo. -->
- Ícones de categoria em emoji (`🐣 🏗️ ✨ ⚡ 🚀`, `entities/dish/model/categories.ts`) aparecem no cardápio, nas abas e
  nos cards. O `copy-lint` não os vê (campo `icon`). Decidir: ícone do `lucide-react` (ajuste 4) ou exceção de identidade.
- O `copy-lint` só varre `frontend/src`: o `index.html` e as strings do servidor (`DatabaseSeeder`) ficam fora do gate.
- O app só tem tema escuro (`class="dark"` fixo no `index.html`), embora os tokens claros existam. Decidir se o tema
  claro é requisito; se não, os tokens claros são código morto.
- O `gate.sh` não roda no CI. Candidato a passo no job `frontend` (ADR 0017).
