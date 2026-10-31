# 0030. Dono do pedido por token e status com transições fechadas

- **Status:** Aceito (fluxo autônomo, ADR 0026)
- **Data:** 2026-10-31

## Contexto
O ADR 0009 deixou um custo aceito: qualquer pessoa com o id de um pedido o cancela ou altera (`PATCH /cancel` e `PUT`
são públicos), e os ids aparecem em telas públicas (comanda, acompanhamento). O cliente não tem login: comanda é mesa +
nome (ADR 0014), e nomes aparecem na lista de comandas da mesa (spec 007), então "saber o nome" não prova nada. Além
disso, `PATCH /status` aceita qualquer transição, como `Cancelled` para `Done`.

## Decisão
- **Token de dono:** ao criar um pedido, a API gera 256 bits aleatórios (base64url), devolve o token **só nessa
  resposta** (`ownerToken`) e guarda apenas o SHA-256 em `Orders.OwnerTokenHash`. Alterar (`PUT`) e cancelar exigem o
  cabeçalho `X-Order-Token` com esse token, comparado em tempo constante, **ou** o JWT da cozinha (`Admin`). Sem isso:
  403 (`sem-permissao`) com o motivo em português. Pedidos antigos, sem hash, só a cozinha muda.
- **O frontend guarda o token por aparelho** (`localStorage`, `entities/order/model/ownerTokens`) e o manda no
  cancelamento. Trocar de aparelho perde o poder de alterar; a cozinha continua podendo.
- **Status com transições fechadas no domínio** (`Order.CanMoveTo`): Created vai para Draft, Done ou Cancelled; Draft
  vai para Draft, Created, Done ou Cancelled; Done e Cancelled são finais. O serviço recusa com 400 (`regra`) e mensagem
  em português; o domínio ainda lança se alguém pular o serviço.
- Substitui em parte o ADR 0009 (o custo "qualquer um cancela") e mantém o resto (JWT da cozinha, papel Admin).

## Alternativas consideradas
- **Exigir o nome da comanda para cancelar:** o nome é público na lista de comandas; não prova dono.
- **Login de cliente:** resolve de vez, mas muda o produto (pedido sem cadastro é o conceito). Fica para quando houver
  identidade de cliente.
- **Token por comanda em vez de por pedido:** daria poder sobre pedidos futuros de quem só viu um link; por pedido é o
  menor poder possível.

## Consequências
- ✅ Ver um id ou um nome não dá mais poder de cancelar ou alterar.
- ✅ Status não volta de servido ou cancelado, e a auditoria (ADR 0027) mostra só transições válidas.
- ⚠️ O token mora no aparelho: limpar o navegador ou trocar de celular tira do cliente o poder de mudar o pedido.
- ⚠️ Quem copia o `localStorage` de alguém copia o poder; o risco é o mesmo de qualquer sessão no navegador.
