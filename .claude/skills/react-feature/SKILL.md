---
name: react-feature
description: Receita para criar ou alterar código no frontend React do Software Hamgueria (Feature-Sliced Design com app, pages, widgets, features, entities e shared; Atomic Design + shadcn/ui em shared/ui; Tailwind v4; TanStack Query; Zustand; MSW). Use sempre que for criar ou alterar telas, componentes, chamadas de API, hooks, stores, rotas ou mocks em frontend/.
---

# Skill: react-feature

Regras completas nos ADRs 0011 (FSD + shadcn), 0012 (estado), 0013 (MSW) e 0007/0008 (contrato da API).
Os exemplos usam `dish` e a ação `close-comanda`. **Copie o padrão existente.**

## Regra de ouro do FSD
Os imports só vão **para baixo**: `app → pages → widgets → features → entities → shared`.
Uma slice não importa outra slice da mesma camada. Toda slice exporta só pelo seu `index.ts`, e quem consome importa
de `@/<camada>/<slice>` (nunca de `@/entities/dish/model/types` de fora da slice).
**O lint não verifica isso** (ADR 0011): confira os imports à mão antes de marcar a task.

## Ordem dos passos

### 1. `shared/mocks/`: handler MSW (ADR 0013)
Antes da tela, o endpoint precisa existir no mock com os mesmos status do backend (sucesso **e** erros).
Rotas específicas (`/api/order/comanda`) **antes** de `/:id`. Estado que precisa ser visto em outra aba vai em
`localStorage`, como os pedidos atuais.

### 2. `entities/<x>/`: modelo + API
```
entities/dish/
├── model/types.ts        // espelha o ResponseDto do backend (camelCase; enums como union de strings)
├── api/dishApi.ts        // funções de fetch + hooks de leitura
├── ui/DishCard.tsx       // representação visual da entidade (opcional)
└── index.ts
```
```ts
// api/dishApi.ts
export async function fetchDishes(): Promise<Dish[]> {
  const { data } = await httpClient.get<Dish[]>('/api/dish')
  return data
}

export function useDishes() {
  return useQuery({ queryKey: queryKeys.dishes, queryFn: fetchDishes })
}
```
- Sempre o `httpClient` de `@/shared/api/httpClient` (anexa o JWT da cozinha).
- Chave nova? Acrescente em `shared/api/queryKeys.ts`; nunca escreva o array da chave inline.
- Telas "ao vivo": `refetchInterval` que para em status terminal (veja `entities/order/api/orderApi.ts`).
- Funções de escrita (`createOrder`, `closeComanda`) ficam na entity como funções simples; o `useMutation` vai na feature.

### 3. `features/<acao>/`: uma ação do usuário
```
features/close-comanda/
├── model/useCloseComanda.ts
├── ui/...                // botão/diálogo da ação, se houver
└── index.ts
```
```ts
export function useCloseComanda() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ tableId, customerName }: { tableId: string; customerName: string }) =>
      closeComanda(tableId, customerName),
    onSuccess: (_data, { tableId, customerName }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.comanda(tableId, customerName) })
      queryClient.invalidateQueries({ queryKey: queryKeys.tableOrders(tableId) })
      toast.success('Comanda fechada.')
    },
    onError: () => toast.error('Não foi possível fechar a comanda.'),
  })
}
```
- Nome da slice = verbo + objeto em kebab-case (`submit-order`, `select-table`, `manage-order`).
- Invalide **todas** as chaves que a mutação afeta (liste-as no `plan.md`).
- Feedback com `toast` do `sonner`, em português.
- Botões com `disabled={mutation.isPending}` para não enviar duas vezes.

### 4. `widgets/<bloco>/`: composição de entities + features
Blocos grandes de tela reaproveitáveis (`header`, `menu-catalog`, `order-json`). Recebem dados por props ou hooks;
não definem rota.

### 5. `pages/<pagina>/` + rota
`pages/<pagina>/ui/<Pagina>Page.tsx` + `index.ts`, e a rota em `app/router/routes.tsx`. Rotas em português quando são
do cliente (`/comanda`, `/mesa`, `/producao`). Página autenticada usa o `useAuthStore` de `features/auth` (veja `ProductionPage`).

### 6. Componentes visuais (`shared/ui`)
- Primitivo novo do shadcn: `npx shadcn@latest add <componente>` (cai em `shared/ui/shadcn/`) e exporte no `index.ts`.
- Átomo/molécula do tema (painel, prompt de terminal): `shared/ui/atoms` ou `shared/ui/molecules`, exportado no `index.ts` da pasta.
- Estilo só com Tailwind e os tokens de `app/styles/index.css`; combine classes com `cn()`. Nada de CSS solto ou cor
  hardcoded fora dos tokens. Token novo: variável em `:root` **e** `.dark` + mapeamento em `@theme inline`.
- Tema escuro sóbrio (zinc) com emerald (`brand`) só em destaque pontual (ADR 0011). Não espalhe verde.
- Classe Tailwind nunca montada por concatenação (`` `text-${tom}` ``): use um mapa de classes completas
  (veja `TONE_BADGE` em `entities/order/ui/StatusBadge.tsx`).

## Regras gerais
- Componentes função com export nomeado (`export function MenuPage()`), sem `default export`.
- Nada de `any`. Tipos vêm de `entities/*/model`.
- Estado do servidor só no TanStack Query; estado do cliente só no Zustand ou `useState` (ADR 0012).
- **Antes de desenhar uma tela:** classifique a superfície e passe o *cut test* da `visant-frontend` (ADR 0015).
  Animação/loading: `visant-motion`. Gráfico/medidor: `visant-refine`.
- Textos de interface em português, no tom satírico dev do projeto: proibições da `visant-copy` (sem travessão,
  emoji, ponto-e-vírgula, "não é X, é Y") + microcopy da `better-writing`.
- Seta em texto (`→`) nunca: ícone do `lucide-react`. Nada de `hover:scale-*` nem `transition-all`.
- Acessibilidade: siga a skill `better-accessibility` (labels, foco visível, teclado, alvos de toque, `aria-live`
  para status que muda sozinho, como o acompanhamento do pedido).
- Utilities e tokens: skill `tailwind-v4`. Polimento visual (sombras, bordas, animação): `better-ui`.
- Performance de render: `vercel-react-best-practices`, respeitando a precedência do `CLAUDE.md`.

## Verificação
```bash
cd frontend && npm test && npm run build && npm run lint   # Vitest ao lado da slice (`*.test.tsx`), com sucesso e falha
```
E manual: `npm run dev` com mocks (padrão) e, se a spec mexeu no backend, com `VITE_ENABLE_MOCKS=false` + API rodando.
Cada task inclui sucesso e falha (Vitest com `renderWithProviders`/`server` de `@/shared/test`; fluxo novo de tela ganha um passo em `e2e/`).

## Checklist de pronto
- [ ] Handler MSW com sucesso e erros, na ordem certa
- [ ] `entities/<x>` com `types.ts`, API e `index.ts`; chaves em `queryKeys.ts`
- [ ] Mutação em `features/<acao>`, invalidando todas as chaves afetadas, com toast de sucesso e erro
- [ ] Imports só para baixo e só pelos `index.ts`
- [ ] Rota registrada (se for página)
- [ ] Superfície classificada e *cut test* feito (`visant-frontend`)
- [ ] `killer-scan`, `copy-lint` e `motion-lint` limpos nos arquivos tocados (ou exceção escrita na linha)
- [ ] Tela passou pelo checklist de `better-accessibility`
- [ ] `npm run build` e `npm run lint` verdes; testado com mock e (se aplicável) com a API real
