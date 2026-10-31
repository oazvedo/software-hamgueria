# 014. Tema claro: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O Hamgueria só tinha o tema escuro, preso no código. Quem usa no claro do dia (mesa perto da janela, cozinha iluminada)
ou prefere tema claro no sistema não tinha escolha. Pedido do usuário em 2026-10-31.

## Critérios de aceite
- **CA-01:** Dado alguém sem escolha feita, quando abre o sistema, então o tema segue o do sistema operacional, e
  acompanha se o sistema mudar.
- **CA-02:** Dado o botão de tema no topo (cliente e cozinha), quando a pessoa toca, então o tema troca e a escolha fica
  guardada no aparelho.
- **CA-03:** Dada uma escolha feita, quando o sistema muda de tema, então a escolha da pessoa continua valendo.
- **CA-04:** Dada uma escolha guardada, quando a pessoa volta, então a página já abre no tema certo, sem piscar o outro.
- **CA-05:** Dado o tema claro, então todo texto tem contraste suficiente para leitura (4,5:1 no texto pequeno).

## Fora de escopo
- Mais temas. Tema por usuário no servidor.
