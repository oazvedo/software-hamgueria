# 006. Telas limpas, com a identidade dev só onde ela ajuda: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0011, 0013, 0015 (com os ajustes 1 a 10 do `CLAUDE.md`), 0016
- **ADRs novos propostos:** nenhum. Tokens novos de cor seguem o padrão do shadcn já usado (`:root` + `.dark` +
  `@theme inline`). Motion só troca `transition-all` por propriedades nomeadas (ajuste 5: sem SSoT de motion ainda).

## Abordagem
Correções mecânicas, arquivo por arquivo, guiadas pelos 4 detectores (`killer-scan` com cor e ruído, `copy-lint`,
`motion-lint`) rodados em `frontend/src`. As exceções de identidade (RN-02) usam o formato que o `noise-scan` aceita
na linha ou até 8 linhas acima: `{/* EXCEÇÃO ao ruido-scan/code-crumb: <motivo com 8+ letras> */}`. O mono da cozinha
(RN-04) passa por um átomo novo, `DataText`, que é o único lugar com `font-mono tabular-nums` para dado tabular, e a
regra "mono só em dado" fica no código, não em 10 exceções. Um script `scripts/ui/gate.sh` roda os 4 detectores de uma
vez e falha se algum achar algo; `scripts/ui/gate.test.sh` prova que ele ainda pega tiques novos (CA-03). Testes de
interface cobrem cada tela tocada; o e2e confere o idioma. No fim, a auditoria do `visant-killer` com as telas abertas.

## Modelo de dados
Sem mudança de schema. O `DatabaseSeeder` muda 2 descrições (só bancos novos). Sem migration.

## Contrato da API
Sem mudança. Handlers MSW: `shared/mocks/data.ts` muda as mesmas 2 descrições, para mock e servidor dizerem igual.

## Textos novos (RN-06)
| Onde | Hoje | Novo |
|---|---|---|
| `entities/dish/model/categories.ts` (4) | `Entradas — baratos, em grande quantidade...` | `Entradas: baratos, em grande quantidade...` (mesmo padrão nas 4) |
| `entities/order/model/orderStatus.ts` | `deploy concluído 🚀` | `deploy concluído` |
| `features/submit-order/ui/CheckoutDialog.tsx` | `➕ nova comanda` | `Nova comanda` com ícone `Plus` (`aria-hidden`) |
| `pages/order-status/ui/OrderStatusPage.tsx` | `Pedido servido — deploy em produção concluído. Bom apetite!` | `Pedido servido. Deploy em produção concluído, bom apetite!` |
| `pages/production/ui/ProductionPage.tsx` (erro) | `...Sessão pode ter expirado — saia e entre novamente.` | `Não foi possível carregar os pedidos. Sua sessão pode ter expirado: saia e entre de novo.` |
| `pages/production/ui/ProductionPage.tsx` (vazio) | `Nenhum pedido na fila. 🎉` | `Nenhum pedido na fila. A cozinha está em dia.` |
| `shared/mocks/data.ts` + `DatabaseSeeder.cs` | `150ml. Copo americano — é o que tem no orçamento.` / `500ml. Pint cheia — você já sabe...` | `150ml. Copo americano, que é o que cabe no orçamento.` / `500ml. Pint cheia: você já sabe o que está fazendo.` |
| `widgets/order-json/ui/OrderJsonPanel.tsx` | `// selecione uma mesa para enviar o pedido` (mono) | `Selecione uma mesa para enviar o pedido.` (texto comum) |
| `pages/comanda/ui/ComandaPage.tsx` | `// comanda fechada — abra uma nova pelo cardápio` (mono) | `Comanda fechada. Para pedir de novo, abra uma nova pelo cardápio.` (texto comum) |

## Frontend: arquivos
- `index.html`: `lang="pt-BR"`.
- `app/styles/index.css`: `--window-close`, `--window-minimize`, `--window-zoom` (oklch equivalentes aos hex atuais,
  iguais em `:root` e `.dark`) + `--color-window-*` no `@theme inline`.
- `shared/ui/shadcn/button.tsx`: `transition-all` → `transition-[color,background-color,border-color,box-shadow]`.
- `shared/ui/atoms/WindowDots.tsx` (novo): as 3 bolinhas com os tokens, `aria-hidden`. Usado no `CheckoutDialog` e no
  `OrderJsonPanel` (some a duplicação).
- `shared/ui/atoms/DataText.tsx` (novo): `<span className="font-mono tabular-nums">` para número, id, horário e
  quantidade. Exportado no `index.ts` do `atoms`.
- `entities/order/ui/OrderCard.tsx`: `↳` → `CornerDownRight` (`aria-hidden`); `ver detalhe →` → `Ver detalhe` +
  `ChevronRight` (`aria-hidden`); quantidade com `DataText`.
- `pages/order-status/ui/OrderStatusPage.tsx`: `↳` → ícone; copy; exceções de `// pipeline` e `// itens`.
- `pages/production/ui/ProductionPage.tsx`: `↳` → ícone (2); mono só em dado via `DataText` (tempo, quantidade, total,
  `order/<id>`, contagens); `N em produção` e nomes na fonte normal; copy de erro e de vazio; exceção de `// cozinha`.
- `pages/menu/ui/MenuPage.tsx`, `features/auth/ui/LoginForm.tsx`, `widgets/header/ui/Header.tsx`: exceções escritas
  (`code-crumb`) nos rótulos e na nav `~/`.
- `pages/comanda/ui/ComandaPage.tsx`, `widgets/order-json/ui/OrderJsonPanel.tsx`: instrução em texto comum.
- `entities/dish/model/categories.ts`, `entities/order/model/orderStatus.ts`, `features/submit-order/ui/CheckoutDialog.tsx`,
  `shared/mocks/data.ts`: copy.
- Testes (Vitest + Testing Library + MSW):
  - `entities/order/ui/OrderCard.test.tsx` (novo): CA-05, CA-06 (ícone com `aria-hidden`, texto "Ver detalhe", sem `→`/`↳`).
  - `pages/production/ui/ProductionPage.test.tsx` (novo): CA-07 (dado em `font-mono tabular-nums`, nome do prato e
    observação sem `font-mono`), CA-08 (vazio sem emoji), CA-09 (erro com a copy nova, sem travessão).
  - `pages/comanda/ui/ComandaPage.test.tsx` (novo): CA-04 (comanda fechada, instrução sem `//`).
  - `widgets/order-json/ui/OrderJsonPanel.test.tsx`: CA-04 (sem mesa) e CA-10 (bolinhas `aria-hidden`, classes de token).
  - `features/submit-order/ui/CheckoutDialog.test.tsx`: CA-08 (opção "Nova comanda" sem emoji) e CA-10.
  - `pages/order-status/ui/OrderStatusPage.test.tsx`: CA-05, CA-08 (servido sem travessão nem emoji).
  - `entities/order/model/orderStatus.test.ts`: dica de `Done` sem emoji.
  - `e2e/cliente.spec.ts`: CA-01 (`html[lang="pt-BR"]`) e CA-11 (a suíte inteira continua passando).

## Backend: arquivos
- `Infrastructure/Data/DatabaseSeeder.cs`: as 2 descrições (mesmo texto do mock). Sem teste novo: o seed não tem teste
  hoje e a mudança é só de texto; o `copy-lint` do gate não varre o backend, então a checagem é o `grep` da task.

## Raiz
- `scripts/ui/gate.sh` (novo): roda `killer-scan`, `copy-lint` e `motion-lint` em `frontend/src` (com
  `npm_config_yes=false`) e sai com erro se qualquer um achar algo. Aceita outro alvo como argumento.
- `scripts/ui/gate.test.sh` (novo, estilo `convencao.test.sh`): copia uma amostra limpa e uma suja (travessão, emoji,
  `→` em texto, hex cru, `transition-all`) para um diretório temporário e confere que o gate passa na limpa e falha na
  suja.
- `CLAUDE.md`: o comando `sh scripts/ui/gate.sh`; o ajuste 9 passa a citar a spec 006 como a decisão tomada.
- `docs/specs/README.md`: saem os candidatos "Limpeza da linha de base Visant", "Copy existente", "Identidade dev" e
  "`lang=pt-BR`".
- `docs/audits/2026-10-31-frontend.md`: relatório do `killer-scan --init-report/--report` com a tabela de superfícies
  e as telas abertas (`verify-screens.mjs` com o Vite em mock).

## Critérios de aceite → verificação
| CA | Como verificar | Detalhe |
|---|---|---|
| CA-01 | E2E | `cliente.spec.ts`: `html` com `lang="pt-BR"` |
| CA-02 | Script | `sh scripts/ui/gate.sh` → sai 0; exceções com motivo (o próprio `noise-scan` recusa exceção sem motivo) |
| CA-03 | Script | `sh scripts/ui/gate.test.sh` → amostra suja reprovada em cada detector |
| CA-04 | Interface | `OrderJsonPanel.test.tsx`, `ComandaPage.test.tsx` |
| CA-05 | Interface | `OrderCard.test.tsx`, `OrderStatusPage.test.tsx`, `ProductionPage.test.tsx` |
| CA-06 | Interface | `OrderCard.test.tsx` |
| CA-07 | Interface | `ProductionPage.test.tsx` |
| CA-08 | Interface | `CheckoutDialog.test.tsx`, `OrderStatusPage.test.tsx`, `ProductionPage.test.tsx`, `orderStatus.test.ts` |
| CA-09 | Interface | `ProductionPage.test.tsx` (handler MSW de `/api/order` devolvendo 401) |
| CA-10 | Interface + auditoria | `CheckoutDialog.test.tsx`, `OrderJsonPanel.test.tsx`; tema claro e escuro conferidos nas telas abertas |
| CA-11 | E2E + suíte | `npm run e2e` e `npm test` verdes |

## Riscos e decisões pendentes
- `DataText` poderia parecer um jeito de "esconder" o mono do detector. Não é: ele restringe o mono a um uso nomeado
  (dado tabular), que é a regra RN-04. Mono fora dele em texto vira achado de novo.
- O `copy-lint` também lê comentários e strings de teste: textos de teste que citam o tique antigo (ex.: "sem 🎉")
  devem testar por regex (`/\p{Extended_Pictographic}/u`), não com o caractere literal.
- A auditoria com telas abertas depende do Playwright instalado (já está, pelo e2e).
