# 008. Cardápio sem emoji e mais fácil de percorrer: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O cardápio usa emoji como ícone de categoria (fora da voz do produto e anunciados pelo leitor de tela), filtra por
abas que escondem o resto do cardápio, repete a categoria dentro de cada prato e mostra pratos que a cozinha não faz
mais (o cliente só descobre no envio). Depois de adicionar um prato, ajustar a quantidade exige ir até o painel do pedido.

## Objetivo
O cliente percorre o cardápio inteiro numa página, pula para uma categoria com um toque, vê só o que pode pedir e ajusta
a quantidade no próprio prato.

## Regras de negócio
- RN-01: nenhum emoji no cardápio; cada categoria tem um ícone desenhado, que o leitor de tela não anuncia.
- RN-02: todas as categorias ficam na mesma página; uma barra fixa de atalhos leva a cada uma e marca a que está na tela.
- RN-03: prato inativo não aparece; categoria sem prato ativo também não.
- RN-04: depois de adicionar, o próprio prato mostra a quantidade com "mais um" e "tirar um" (até 20). Tirar o último
  tira o prato do pedido.
- RN-05: se o cardápio não carregar, a tela diz o que houve em português, sem expor detalhes técnicos.

## Critérios de aceite
- **CA-01:** Dado o cardápio, quando o cliente o abre, então não há emoji, e os ícones de categoria não são anunciados.
- **CA-02:** Dado o cardápio, quando o cliente toca numa categoria da barra, então vai até a seção dela, na mesma página.
- **CA-03:** Dado um prato inativo e uma categoria só com inativos, quando o cliente abre o cardápio, então nenhum dos
  dois aparece.
- **CA-04:** Dado um prato adicionado, quando o cliente usa "mais um" e "tirar um" no prato, então a quantidade muda no
  pedido; tirando o último, o prato sai e o botão "Adicionar" volta.
- **CA-05 (falha):** Dado o cardápio fora do ar, quando o cliente abre a página, então vê "Não foi possível carregar o
  cardápio", sem rota da API na tela.

## Fora de escopo
- Busca, fotos e filtros (vegetariano etc.). Gestão de pratos pela cozinha na tela.
