# 003. Validação de entrada e erros padronizados: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Hoje o Hamgueria aceita quase qualquer coisa. Um pedido sem itens, com quantidade zero ou negativa vira um pedido
recebido na cozinha. Um prato com nome vazio ou preço negativo entra no cardápio. Uma observação ou um nome de comanda
longo demais só falha na hora de gravar, com um erro inesperado. Quando algo é recusado, o motivo vem em inglês, num
formato diferente a cada situação (às vezes nem vem), e a tela do cliente mostra sempre o mesmo aviso genérico
("Erro ao enviar o pedido."). Quem sente: o **cliente**, que não sabe o que corrigir; a **cozinha**, que recebe pedidos
sem sentido e cadastra pratos inválidos sem perceber; e o **mantenedor**, que não distingue um erro do cliente de uma
falha do sistema.

## Objetivo
Toda entrada inválida é recusada antes de virar pedido, prato ou mesa, com um motivo em português, por campo, sempre no
mesmo formato. A tela do cliente mostra esse motivo perto de onde ele pode corrigir. Falhas inesperadas aparecem como
uma mensagem neutra, sem expor nada de dentro do sistema.

## Histórias de usuário
- Como **cliente**, quero saber exatamente o que está errado no meu pedido para corrigir e enviar de novo.
- Como **cozinha**, quero que o sistema recuse pratos e mesas com dados inválidos para o cardápio não ficar quebrado.
- Como **mantenedor**, quero que toda recusa siga o mesmo formato para a tela tratar todas do mesmo jeito.

## Regras de negócio
- RN-01 (pedido): tem mesa, nome de comanda e **pelo menos um item**. Cada item tem um prato e quantidade de **1 a 20**,
  somando as repetições do mesmo prato. A observação do item é opcional, com **até 200 caracteres**.
- RN-02 (comanda): o nome é obrigatório, com **até 60 caracteres** depois de tirar os espaços das pontas. Vale para
  enviar pedido, abrir e fechar comanda.
- RN-03 (prato): nome obrigatório com até 200 caracteres, descrição com até 1000, **preço maior que zero** e categoria
  entre as 5 do cardápio.
- RN-04 (mesa): nome obrigatório, com até 100 caracteres.
- RN-05 (alteração de pedido): segue as mesmas regras de itens do RN-01.
- RN-06 (formato): toda recusa traz um título curto e a lista de problemas. Cada problema diz **qual campo** (ex.:
  "nome da comanda", "item 2, quantidade") e **por quê**, em português, com o limite quando houver
  (ex.: "A observação pode ter no máximo 200 caracteres."). Todos os problemas encontrados vêm de uma vez.
- RN-07 (tipos de recusa): o sistema distingue, no mesmo formato, entrada inválida, registro inexistente (mesa, prato,
  pedido), regra do restaurante (prato inativo), sem login ou sem permissão, e falha inesperada.
- RN-08 (falha inesperada): o cliente vê só uma mensagem neutra ("Algo deu errado do nosso lado. Tente de novo.") e
  um código de ocorrência. Nenhum detalhe interno aparece (nome de arquivo, consulta, rastreio de erro). O mantenedor
  encontra o detalhe pelo código.
- RN-09 (modo simulado): com dados simulados, as mesmas entradas são recusadas com as mesmas mensagens e o mesmo formato.

## Critérios de aceite
- **CA-01:** Dado um pedido válido (mesa, nome, itens com quantidade de 1 a 20, observações curtas), quando o cliente
  envia, então o pedido é criado como recebido, como hoje (001/CA-03).
- **CA-02 (falha):** Dado um pedido sem nenhum item, quando é enviado, então é recusado com o problema "itens: o pedido
  precisa de pelo menos um item".
- **CA-03 (falha):** Dado um item com quantidade 0, negativa ou acima de 20, quando o pedido é enviado (ou alterado),
  então é recusado indicando o item e a quantidade, e nada é gravado.
- **CA-04 (falha):** Dada uma observação com 201 caracteres, quando o pedido é enviado, então é recusado indicando a
  observação daquele item e o limite de 200; com exatamente 200, é aceito.
- **CA-05 (falha):** Dado um nome de comanda vazio, só com espaços ou com 61 caracteres, quando o pedido é enviado ou a
  comanda é fechada, então é recusado indicando o nome da comanda e o motivo; com 60 caracteres, é aceito.
- **CA-06 (falha):** Dado um pedido com vários problemas (sem nome e item com quantidade 0), quando é enviado, então a
  recusa lista os dois problemas de uma vez.
- **CA-07 (falha):** Dada a cozinha autenticada, quando cadastra ou altera um prato com nome vazio, preço zero ou
  negativo, nome acima de 200 ou descrição acima de 1000 caracteres, então é recusado indicando cada campo errado, e o
  cardápio não muda.
- **CA-08 (falha):** Dada a cozinha autenticada, quando cadastra ou altera uma mesa com nome vazio ou acima de 100
  caracteres, então é recusado indicando o nome da mesa, e as mesas não mudam.
- **CA-09 (falha):** Dado um prato inativo, uma mesa ou um prato inexistente, quando o pedido é enviado, então é
  recusado no mesmo formato, com o motivo em português (ex.: "O prato Burger Monolito não está disponível.").
- **CA-10 (falha):** Dada uma falha inesperada no sistema, quando qualquer ação é feita, então a resposta tem a
  mensagem neutra e o código de ocorrência, e não contém nenhum detalhe interno.
- **CA-11:** Dada uma recusa por entrada inválida no envio do pedido, quando a tela do cliente a recebe, então mostra a
  mensagem de cada campo junto do campo (ex.: abaixo do nome da comanda) e mantém o carrinho como estava.
- **CA-12:** Dada uma recusa sem campo (prato inativo, mesa inexistente, falha inesperada), quando a tela do cliente a
  recebe ao enviar o pedido ou fechar a comanda, então mostra a mensagem do sistema no aviso, e não o texto genérico.
- **CA-13:** Dado o app em modo simulado, quando os cenários CA-02 a CA-09 são repetidos, então as recusas têm as
  mesmas mensagens e o mesmo formato do servidor real.
- **CA-14:** Dada a cozinha autenticada, quando finalizar ou cancelar um pedido é recusado, então o aviso mostra a
  mensagem do sistema, e não o texto genérico.

## Impacto na linha de base
- **001/CA-04:** continua recusado. Muda a resposta: o motivo passa a vir em português, indicando o campo "nome da
  comanda", no formato do RN-06. O limite de 60 caracteres passa a ser checado no envio (antes falhava ao gravar).
- **001/CA-05:** continua recusado com o motivo. O motivo passa a vir em português e no formato do RN-06.
- **001/CA-10:** credenciais erradas continuam negadas. A resposta passa a usar o formato do RN-07.
- **001/CA-03:** inalterado para pedidos válidos. Pedidos sem itens ou com quantidade fora de 1 a 20, hoje aceitos,
  passam a ser recusados.
- Os testes da 002 que conferem esses CAs precisam ser ajustados para o novo formato da recusa.

## Fora de escopo
- Unit of Work e atomicidade entre gravar o pedido e avisar outros sistemas (candidato próprio).
- Mudanças em login, permissões ou dono da comanda (cancelamento por qualquer pessoa, 001/CA-12).
- Preço guardado com precisão de dinheiro (candidato "Preço como `decimal`").
- Transições de status válidas e esconder pratos inativos do cardápio (candidatos próprios).
- Telas de gestão de cardápio e mesas para a cozinha (continuam fora, como na 001).
- Reescrever os avisos antigos que não vêm do servidor (candidato "Copy existente nas regras da `visant-copy`").
- Tradução para outros idiomas: as mensagens são só em português.

## Decisões (dúvidas resolvidas em 2026-10-31)
- **Quantidade por item:** de 1 a 20. O limite vale também para a soma quando o mesmo prato aparece duas vezes no
  pedido (001/RN-04).
- **Avisos da cozinha:** finalizar e cancelar pedido também mostram a mensagem do sistema, no mesmo bloco do CA-12
  (CA-14).
- **Espaços nas pontas:** nomes de comanda, prato e mesa são aparados antes de checar vazio e tamanho, e gravados
  aparados.
