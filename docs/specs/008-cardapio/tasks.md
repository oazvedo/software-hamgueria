# 008. Cardápio sem emoji e mais fácil de percorrer: Tasks

- [x] **T01. Ícones de categoria do lucide** (`categories.ts`) · CA-01
- [x] **T02. Card com slot de ação e stepper no prato** (`DishCard`, `AddDishButton`, `QuantityStepper`) · CA-04
- [x] **T03. Cardápio numa página com barra de atalhos e sem inativos** (`MenuCatalog`, `CategoryTab`) · CA-02, CA-03, CA-05
- [x] **T04. Testes, gate, e2e** · `npm test -- MenuCatalog && sh scripts/ui/gate.sh && npm run e2e`

## Backlog
- A API pública devolve pratos inativos (o front filtra). Um `GET /api/dish?ativos=true` evitaria mandar o que não se mostra.
- Correções da revisão do PR #8 (spec 007) entraram aqui: ordenação de comandas por instante e asserções de totais no teste.
