# 008. Cardápio sem emoji e mais fácil de percorrer: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0011 (inclusive a regra de imports: entity não importa feature), 0012, 0013, 0015, 0016, 0026
- **ADRs novos:** nenhum

## Abordagem
Só frontend. `CATEGORY_META.icon` vira um componente do `lucide-react` (Egg, Layers, Leaf, Zap, Rocket). O
`MenuCatalog` deixa de filtrar por aba: renderiza todas as seções e uma barra fixa de links `#cardapio-<slug>`, com a
seção na tela marcada por `IntersectionObserver` (sem ele, nos testes, a primeira). Pratos inativos saem no front (a API
pública continua devolvendo todos; a cozinha não tem tela de pratos). O `DishCard` recebe a ação por slot (corrige o
import de feature dentro de entity) e o `AddDishButton` vira `QuantityStepper` depois do primeiro toque, com nomes
acessíveis por prato para não colidir com o stepper do painel.

## Verificação
| CA | Como |
|---|---|
| CA-01 a CA-05 | Interface: `MenuCatalog.test.tsx` |
| geral | `sh scripts/ui/gate.sh`, `npm test`, `npm run e2e` (o e2e de 003/CA-03 usa o "Aumentar" do painel, que segue único) |
