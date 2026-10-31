# 002. Base de testes automatizados: Spec

- **Status:** Em andamento
- **Data:** 2026-10-31

## Problema
Hoje nenhuma regra do Hamgueria é verificada automaticamente. Quem mantém o sistema só descobre que quebrou o pedido,
a comanda ou o login da cozinha abrindo o app e clicando, e a verificação das próximas specs se resume a "compila".
Quem sente: o mantenedor (a cada mudança) e, no fim, o cliente e a cozinha, que recebem a regressão.

## Objetivo
Cada comportamento da linha de base (spec 001) passa a ter uma checagem automática que roda com um comando por parte do
sistema e fica vermelha quando o comportamento quebra. As próximas specs ganham onde pendurar os próprios critérios.

## Histórias de usuário
- Como **mantenedor**, quero rodar um comando e saber em poucos minutos se pedido, comanda e cozinha continuam
  funcionando, para mudar o código sem medo.
- Como **mantenedor**, quero que a checagem me diga **qual** regra quebrou, para não depurar às cegas.
- Como **mantenedor**, quero que as checagens não dependam nem mexam nos dados que uso no dia a dia, para rodar a
  qualquer hora.

## Regras de negócio
- RN-01: Todo critério de aceite da spec 001, exceto os marcados com ⚠️ (CA-01 e CA-12), tem pelo menos uma checagem
  automática de **sucesso**; os de falha (CA-04, CA-05, CA-09, CA-10) têm a checagem do caso recusado.
- RN-02: As checagens criam os próprios dados e não leem nem alteram os dados do ambiente de desenvolvimento.
- RN-03: O resultado é determinístico: rodar duas vezes seguidas sem mudar nada dá o mesmo resultado.
- RN-04: As checagens das regras do servidor não dependem da interface, e as da interface não dependem do servidor
  estar no ar (exceto a checagem de ponta a ponta, que roda nos dois modos: simulado e real).

## Critérios de aceite
- **CA-01:** Dado o projeto com os pré-requisitos instalados, quando o mantenedor roda a checagem do servidor, então
  as regras de pedido, comanda e acesso da cozinha (CA-03, 04, 05, 07, 08, 09, 10, 11 da 001) são verificadas e o
  resultado sai em até 3 minutos.
- **CA-02:** Dado o projeto, quando o mantenedor roda a checagem da interface, então montagem do pedido, painel JSON
  e acompanhamento (CA-02, 03, 06 da 001) são verificados sem o servidor no ar, em até 1 minuto.
- **CA-03:** Dado o app rodando com dados simulados, quando o mantenedor roda a checagem de ponta a ponta, então o
  fluxo cliente (cardápio → pedido → acompanhamento → comanda → fechar comanda) e o fluxo cozinha (login → finalizar
  pedido) passam pelo navegador (CA-07, 08, 11, 13 da 001).
- **CA-04 (falha):** Dada uma regra da linha de base quebrada de propósito (ex.: aceitar pedido sem nome de comanda),
  quando a checagem correspondente roda, então ela fica vermelha e a mensagem nomeia a regra (ex.: "pedido sem nome
  de comanda deveria ser recusado").
- **CA-05 (falha):** Dado um banco de desenvolvimento com pedidos, quando a checagem do servidor roda, então os pedidos
  do banco de desenvolvimento continuam iguais antes e depois.
- **CA-06:** Dada uma checagem que passou, quando ela roda de novo sem mudança no código, então passa de novo (sem
  dependência de ordem, horário ou resíduo da rodada anterior).
- **CA-07:** Dada uma spec nova, quando o mantenedor segue o fluxo de implementação, então a verificação de cada task
  é a checagem automática da parte tocada, e não só "compila".

## Impacto na linha de base
Nenhum comportamento muda. A spec 001 ganha uma coluna de cobertura (qual checagem cobre cada CA).

## Fora de escopo
- Corrigir os comportamentos ⚠️ da 001 (pratos inativos no cardápio, cancelamento sem dono): eles têm specs próprias
  em "Candidatos a spec" e ganham checagem quando forem corrigidos.
- Execução automática a cada push (integração contínua): fica para depois que a suíte estiver estável localmente.
- Meta de porcentagem de cobertura de código.
- Checagem visual de tela (aparência), carga e desempenho.
- Mensageria: verificar que o evento de pedido foi recebido por outro sistema (hoje não há consumidor ativo).

## Decisões da revisão (2026-10-31)
- A ponta a ponta roda nos dois modos: **simulado** sempre; **real** (servidor + banco) opcional, só quando o servidor
  está no ar, e sem tocar nos dados de desenvolvimento (RN-02).
- CA-01 e CA-12 da 001 ficam sem checagem até as specs que os corrigem, para não travar como "correto" um defeito.

## Dúvidas em aberto
- Nenhuma.
