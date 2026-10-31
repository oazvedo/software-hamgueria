# 0011. Frontend React com FSD, Atomic Design e shadcn/ui

- **Status:** Aceito
- **Data:** 2026-10-31 (retroativo)

## Contexto
O `frontend/frontend-requirements.md` pediu React + TypeScript, Feature-Sliced Design com Atomic Design pontual nos
componentes visuais, Tailwind e um visual "matrix" (fundo preto, verde). Na implementação o visual evoluiu para um
tema **escuro sóbrio** (neutros zinc, `class="dark"` fixo no `index.html`) com emerald como acento **raro** (`--brand`)
e tipografia mono (JetBrains Mono) para os elementos de terminal.

## Decisão
- **React 19 + TypeScript + Vite**, alias `@/` → `src/`, React Router.
- **Feature-Sliced Design:** `app → pages → widgets → features → entities → shared`. Imports só para baixo; uma slice
  não importa outra da mesma camada; cada slice exporta só pelo `index.ts` e quem consome importa do `index.ts`.
  Segmentos dentro da slice: `api/`, `model/`, `ui/`.
- **Atomic Design só em `shared/ui`:** `atoms/` (ex.: `NeonPanel`), `molecules/` (ex.: `QuantityStepper`) e `shadcn/`
  (primitivos gerados pelo shadcn CLI, configurados em `components.json`).
- **Estilo:** Tailwind v4 com tokens no padrão shadcn em `app/styles/index.css` (variáveis em `:root` e `.dark`,
  mapeadas em `@theme inline`), `cn()` de `shared/lib`, ícones `lucide-react`, toasts `sonner`. Emerald só em
  destaque pontual; o resto é neutro.
- Lint com **oxlint** (`npm run lint`) e checagem de tipos no `npm run build`.

## Alternativas consideradas
- **MUI:** citado no requisito, mas o visual terminal customizado é mais natural com Tailwind + primitivos sem estilo (Radix).
- **Estrutura Bulletproof React:** parecida, mas o FSD tem regras de camada mais explícitas.

## Consequências
- ✅ Cada arquivo tem um lugar óbvio e a skill `react-feature` vira uma receita.
- ⚠️ A regra de imports do FSD **não é verificada por lint** (o oxlint atual não tem regra de fronteiras); depende de revisão.
