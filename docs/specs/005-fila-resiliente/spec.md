# 005. Pedidos funcionam com a fila de mensagens fora do ar: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Todo pedido criado, alterado ou cancelado gera um aviso numa fila de mensagens, para outros sistemas reagirem. Hoje o
restaurante inteiro depende dessa fila. Se ela estiver fora do ar, o cliente envia o pedido, ele é gravado, mas a tela
mostra erro, e o aviso se perde. Pior: até para **ver** um pedido ou a comanda o sistema exige a fila. Na spec 004 isso
apareceu de verdade: depois de reiniciar o servidor, o sistema pode voltar antes da fila, e todas as telas de pedido
quebram até alguém subir tudo de novo à mão. Quem sente: o **cliente**, que vê erro num pedido que deu certo; a
**cozinha**, que fica sem a lista de pedidos; e o **mantenedor**, que precisa intervir a cada reinício.

## Objetivo
A fila vira um detalhe que pode falhar sem derrubar o restaurante. Com a fila fora, clientes e cozinha usam tudo
normalmente, e os avisos esperam para ser enviados quando ela voltar. O mantenedor consegue ver que a fila está fora e
quantos avisos estão esperando.

## Histórias de usuário
- Como **cliente**, quero que meu pedido seja aceito sem erro mesmo que um sistema de bastidor esteja fora.
- Como **cozinha**, quero ver e atualizar os pedidos mesmo com a fila fora do ar.
- Como **mantenedor**, quero reiniciar o servidor sem precisar subir tudo de novo na ordem certa.
- Como **mantenedor**, quero ver se a fila está fora e quantos avisos estão esperando.

## Regras de negócio
- RN-01 (independência): ver cardápio, mesas, pedidos e comandas, e entrar na cozinha, não dependem da fila.
- RN-02 (pedido aceito): com a fila fora, criar, alterar e cancelar pedido funcionam e respondem como se ela estivesse
  no ar. O cliente não vê erro por causa da fila.
- RN-03 (avisos pendentes): o aviso que não pôde ser enviado espera e é enviado sozinho quando a fila volta, **na ordem
  em que os pedidos aconteceram**, sem reiniciar nada.
- RN-04 (limite): no máximo **1000** avisos esperam. Passando disso, o mais antigo é descartado e o descarte fica
  registrado no log com o pedido a que ele se referia.
- RN-05 (perda aceita): se o próprio sistema reiniciar enquanto a fila está fora, os avisos que esperavam se perdem. O
  log registra quantos se perderam ao desligar.
- RN-06 (reconexão): se a fila cair com o sistema no ar, ou o sistema subir antes dela, ele volta a se conectar
  sozinho quando a fila voltar.
- RN-07 (saúde principal): a verificação de saúde da spec 004 continua olhando só o banco. Fila fora não deixa o
  sistema "indisponível".
- RN-08 (saúde detalhada): um segundo endereço de verificação mostra o estado do banco e da fila (disponível ou
  indisponível) e o número de avisos esperando. Ele não exige login e não revela detalhes internos (endereço, usuário,
  mensagem de erro).

## Critérios de aceite
- **CA-01:** Dada a fila no ar, quando o cliente cria, altera ou cancela um pedido, então o aviso é enviado na hora,
  como hoje.
- **CA-02:** Dada a fila fora do ar desde antes do sistema subir, quando o sistema sobe, então ele sobe, e o cliente vê
  o cardápio, a comanda e o acompanhamento do pedido, e a cozinha entra e vê os pedidos.
- **CA-03:** Dada a fila fora do ar, quando o cliente envia, altera ou cancela um pedido, então a operação dá certo
  sem erro, e o aviso fica esperando.
- **CA-04:** Dados avisos esperando, quando a fila volta, então eles são enviados sozinhos, na ordem em que aconteceram,
  e a fila de espera fica vazia.
- **CA-05:** Dado o sistema no ar e conectado, quando a fila cai e depois volta, então os pedidos feitos no meio do
  caminho dão certo, e os avisos deles chegam depois que ela volta.
- **CA-06 (falha):** Dados 1000 avisos esperando, quando chega mais um pedido, então o pedido é aceito, o aviso mais
  antigo é descartado e o log registra o descarte com o pedido dele.
- **CA-07:** Dada a fila fora e o banco no ar, quando alguém consulta a saúde principal, então recebe "saudável"; e
  quando consulta a saúde detalhada, vê o banco disponível, a fila indisponível e o número de avisos esperando.
- **CA-08 (falha):** Dado o banco fora, quando alguém consulta a saúde detalhada, então vê o banco indisponível, e a
  resposta não traz endereço, usuário nem mensagem de erro.
- **CA-09:** Dado o ambiente de produção no ar, quando o mantenedor reinicia tudo de uma vez (sem subir na ordem),
  então, em até 1 minuto, os pedidos voltam a funcionar sem nenhuma ação a mais.

## Impacto na linha de base
Nenhum `CA` da 001 muda. Da 004: o guia de deploy deixa de pedir "rode `up -d --wait` de novo depois de reiniciar", e
sai do backlog da 004 o item da conexão com a fila.

## Fora de escopo
- Garantir que nenhum aviso se perca, inclusive se o sistema reiniciar (gravar o aviso junto com o pedido). Exige
  mudar como o sistema grava (candidato "Unit of Work") e fica para uma spec própria.
- Um leitor dos avisos (nenhum sistema consome a fila hoje, ADR 0010).
- Avisar a mudança de status feita pela cozinha (hoje só o cancelamento avisa, ADR 0010). Continua igual.
- Alertas automáticos para o mantenedor quando a fila cai.

## Dúvidas em aberto
- O limite de 1000 avisos e a reconexão "em até 1 minuto" são suficientes? São fáceis de ajustar no plano.
