# 006. Telas limpas, com a identidade dev só onde ela ajuda: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
As telas do Hamgueria nasceram antes das regras de interface (ADR 0015) e a auditoria de 2026-10-31 achou 46 pontos
nelas. Alguns atrapalham quem usa:
- a página se declara em inglês, e o leitor de tela lê o português com pronúncia inglesa;
- duas instruções importantes ("selecione uma mesa", "comanda fechada, abra uma nova") aparecem com cara de comentário
  de código, que o olho aprende a pular;
- na tela da cozinha, nomes de prato e observações estão na mesma fonte de terminal dos números, o que deixa a leitura
  mais lenta numa tela usada o dia inteiro;
- setas e emojis entram como caracteres soltos no texto, e o leitor de tela anuncia "seta para a direita" ou "festa";
- textos com travessão e emoji fogem da voz definida para o produto.

Outros pontos são decoração com cores fora da paleta e uma animação que mexe em mais propriedades do que deveria.
Quem sente: o **cliente** e a **cozinha**, principalmente quem usa leitor de tela, e o **mantenedor**, que roda o
gate de interface e vê sempre os mesmos achados antigos misturados aos novos.

## Objetivo
O gate de interface passa limpo em todo o frontend. O que é identidade dev (rótulos `// seção`, navegação `~/`, janela
de terminal) fica por decisão escrita. O que atrapalha leitura ou acessibilidade sai. O tom satírico continua.

## Histórias de usuário
- Como **cliente com leitor de tela**, quero que o português seja lido em português e que nada decorativo seja anunciado.
- Como **cliente**, quero que as instruções que me desbloqueiam pareçam instruções, não comentários.
- Como **cozinha**, quero ler nomes e observações dos pedidos rápido, com os números alinhados para comparar.
- Como **mantenedor**, quero que o gate de interface mostre só achados novos.

## Regras de negócio
- RN-01 (idioma): toda página se declara em português do Brasil.
- RN-02 (identidade que fica): os rótulos de seção em forma de comentário (`// monte seu pedido`, `// cozinha`,
  `// pipeline`, `// itens`, `// área da cozinha`) e a navegação `~/menu`, `~/comanda`, `~/mesa` continuam. Cada um
  leva, na própria linha do código, a justificativa escrita de por que fica.
- RN-03 (instrução é texto): as duas instruções ("selecione uma mesa para enviar o pedido" e "comanda fechada, abra
  uma nova pelo cardápio") deixam a forma de comentário e aparecem como texto comum, legível.
- RN-04 (cozinha): na tela da cozinha, a fonte de terminal fica só em dados que se comparam em coluna (número do
  pedido, mesa, quantidade, horário), com algarismos de largura fixa. Nomes de prato, observações, nomes de comanda e
  rótulos usam a fonte normal.
- RN-05 (setas e marcadores): setas e marcadores de observação viram ícones da biblioteca do projeto, escondidos do
  leitor de tela. Nenhuma seta como caractere no texto.
- RN-06 (copy): os 11 textos com travessão ou emoji são reescritos sem eles, mantendo o sentido e o tom satírico.
  Mensagens de erro continuam sérias e diretas. Vale também para as duas descrições de bebida que vêm do cardápio do
  servidor (Chopp Estagiário e Chopp Pleno).
- RN-07 (janela de terminal): as três bolinhas de janela ficam, com cores da paleta do produto, iguais no tema claro
  e no escuro, e escondidas do leitor de tela.
- RN-08 (movimento): os botões animam só as propriedades que mudam de fato (cor e sombra), nunca "todas".
- RN-09 (sem regressão): nenhum fluxo da spec 001 muda de comportamento; só aparência, texto e acessibilidade.

## Critérios de aceite
- **CA-01:** Dada qualquer página do Hamgueria, quando um leitor de tela a abre, então o idioma declarado é português
  do Brasil.
- **CA-02:** Dado o frontend inteiro, quando o mantenedor roda o gate de interface (cor, ruído, copy e movimento),
  então ele passa sem achados, e cada exceção de identidade (RN-02) tem a justificativa escrita na linha.
- **CA-03 (falha):** Dado um texto novo com travessão, emoji ou seta como caractere, ou uma cor fora da paleta, quando
  o gate roda, então ele aponta o problema (o gate continua valendo depois da limpeza).
- **CA-04:** Dado um carrinho com itens e nenhuma mesa escolhida, quando o cliente olha o painel do pedido, então lê
  "Selecione uma mesa para enviar o pedido." como texto comum. Dada uma comanda fechada, a página mostra a instrução de
  abrir uma nova pelo cardápio também como texto comum.
- **CA-05:** Dado um item com observação, em qualquer tela (cartão do pedido, acompanhamento, cozinha), quando é
  exibido, então a observação aparece com um ícone de marcador que o leitor de tela não anuncia, e o texto da
  observação continua sendo lido.
- **CA-06:** Dado o cartão de um pedido, quando o cliente vê o link para o detalhe, então o link diz "Ver detalhe" com
  um ícone ao lado que o leitor de tela não anuncia.
- **CA-07:** Dada a tela da cozinha com pedidos, quando a cozinha a abre, então número, mesa, quantidade e horário
  estão em fonte de terminal com algarismos alinhados, e nomes de prato, observações e comandas na fonte normal.
- **CA-08:** Dado um pedido servido, um carrinho com a opção de nova comanda e a fila da cozinha vazia, quando as
  telas aparecem, então nenhum texto tem emoji e o tom continua satírico.
- **CA-09 (falha):** Dada uma falha ao carregar os pedidos na cozinha, quando a mensagem aparece, então ela é séria e
  direta, sem travessão, e diz o que fazer.
- **CA-10:** Dados os temas claro e escuro, quando o cliente abre o envio do pedido ou o painel do pedido, então as
  três bolinhas da janela aparecem com as mesmas cores da paleta e o leitor de tela não as anuncia.
- **CA-11:** Dado o fluxo completo da spec 001 (cardápio, pedido, comanda, cozinha), quando roda de ponta a ponta com
  dados simulados, então tudo funciona como antes.

## Impacto na linha de base
Nenhum `CA` da 001 muda de comportamento. Mudam textos visíveis (sem emoji e sem travessão), o idioma declarado e a
aparência da tela da cozinha. As duas descrições de bebida mudam no cardápio de um banco **novo**; um banco que já foi
semeado mantém o texto antigo (a semeadura só roda com o banco vazio).

## Fora de escopo
- Quebrar a tela da cozinha em partes menores e carregar as páginas sob demanda (candidatos próprios).
- Redesenho de telas, cores novas de marca ou mudança de layout.
- Corrigir textos já gravados num banco existente.

## Dúvidas em aberto
- Nenhuma: as decisões de identidade (RN-02 a RN-04 e RN-07) foram tomadas em 2026-10-31.
