# 001. Linha de base: o que o Hamgueria já faz: Spec

- **Status:** Concluída (retroativa)
- **Data:** 2026-10-31

> Spec escrita **depois** do código, na adoção do fluxo spec-driven. Ela não tem `plan.md` nem `tasks.md`: as
> decisões técnicas estão nos ADRs 0003 a 0014. Serve como contrato de comportamento: specs futuras não podem quebrar
> estes `CA` sem dizer isso na seção "Impacto na linha de base". Os critérios marcados com ⚠️ descrevem o comportamento
> atual, que pode não ser o desejado (veja "Candidatos a spec").

## Problema
Um restaurante temático para devs precisa de um cardápio digital por mesa, em que cada pessoa monte e acompanhe os
próprios pedidos, e de uma tela para a cozinha produzir, servir e cobrar.

## Objetivo
O cliente pede pelo celular vendo o pedido virar um JSON, acompanha o preparo como um deploy, e a cozinha controla
todos os pedidos e o fechamento das comandas.

## Histórias de usuário
- Como **cliente**, quero ver o cardápio por etapa (Setup, Build, Refactor, Runtime, Deploy) para escolher os pratos.
- Como **cliente**, quero montar o pedido, anotar observações por item e ver o JSON do pedido ao vivo.
- Como **cliente**, quero informar meu nome (comanda) ao enviar, para pagar separado dos outros da mesa.
- Como **cliente**, quero acompanhar meu pedido, minha comanda e a mesa inteira.
- Como **cozinha**, quero entrar com login e ver todos os pedidos por fila ou por mesa, finalizar, cancelar e fechar comandas.

## Regras de negócio
- RN-01: O cardápio tem 5 categorias: entradas, hambúrgueres, saladas, bebidas e sobremesas. Só a cozinha cria,
  altera, ativa e desativa pratos e mesas.
- RN-02: Um pedido pertence a uma mesa e a uma comanda (nome do cliente). O nome é obrigatório.
- RN-03: Não é possível pedir prato inativo nem pedir para mesa ou prato inexistente.
- RN-04: O mesmo prato pedido duas vezes no mesmo pedido vira um item só, com as quantidades somadas.
- RN-05: Um pedido nasce como *recebido* e passa por *em produção* → *servido*; pode ser cancelado.
- RN-06: Comanda = mesa + nome, sem diferenciar maiúsculas. Fechar a comanda marca os pedidos feitos até ali como
  fechados; um pedido novo com o mesmo nome abre uma comanda nova.
- RN-07: Ver todos os pedidos e mudar status manualmente é exclusivo da cozinha autenticada.
- RN-08: Criar, alterar e cancelar pedido gera um evento para outros sistemas.

## Critérios de aceite
- **CA-01 ⚠️:** Dado o cardápio semeado, quando o cliente abre o cardápio, então vê os pratos agrupados por categoria
  (inclusive os inativos, que só são recusados no envio, CA-05).
- **CA-02:** Dado um carrinho com itens, quando o cliente adiciona, muda a quantidade ou escreve uma observação, então
  o painel JSON reflete a mudança na hora.
- **CA-03:** Dado mesa e itens escolhidos, quando o cliente envia informando o nome, então o pedido é criado como
  recebido e o carrinho é esvaziado, mantendo mesa e nome para o próximo pedido.
- **CA-04 (falha):** Dado um pedido sem nome de comanda, quando é enviado, então é recusado.
- **CA-05 (falha):** Dado um prato inativo ou inexistente no pedido, quando é enviado, então é recusado com o motivo.
- **CA-06:** Dado um pedido criado, quando o cliente abre o acompanhamento, então vê o estágio atual, atualizado sozinho
  até o pedido terminar.
- **CA-07:** Dada uma comanda com vários pedidos, quando o cliente abre a comanda, então vê todos os pedidos dela; e
  ao abrir a mesa, vê todas as comandas da mesa.
- **CA-08:** Dada uma comanda aberta, quando o cliente ou a cozinha a fecha, então ela sai das comandas ativas; e um
  pedido novo com o mesmo nome aparece numa comanda nova.
- **CA-09 (falha):** Dado um visitante sem login, quando acessa a tela da cozinha, então vê o formulário de login, e a
  lista completa de pedidos é negada.
- **CA-10 (falha):** Dadas credenciais erradas, quando a cozinha tenta entrar, então o acesso é negado.
- **CA-11:** Dada a cozinha autenticada, quando finaliza ou cancela um pedido, então o cliente vê o novo estado no acompanhamento.
- **CA-12 ⚠️:** Dado um pedido com id conhecido, quando qualquer pessoa pede o cancelamento, então ele é cancelado
  (não há verificação de dono).
- **CA-13:** Dado o frontend sem backend, quando roda em modo padrão, então todo o fluxo acima funciona com dados simulados.

## Cobertura (spec 002)
Busque pelo prefixo `001/CA-xx` no nome do teste. S = servidor (`dotnet test`), I = interface (`npm test`),
E = ponta a ponta (`npm run e2e`, também no modo real).

| CA | Coberto por |
|---|---|
| CA-01 ⚠️ | Sem checagem: comportamento a corrigir ("Esconder pratos inativos do cardápio") |
| CA-02 | I: `orderStore.test.ts`, `orderJson.test.ts`, `OrderJsonPanel.test.tsx` |
| CA-03 | S: `OrderServiceTests`, `OrderEndpointsTests` · I: `orderJson.test.ts`, `orderStore.test.ts`, `useSubmitOrder.test.tsx` |
| CA-04 | S: `OrderServiceTests`, `OrderEndpointsTests` · I: `orderJson.test.ts`, `useSubmitOrder.test.tsx` |
| CA-05 | S: `OrderServiceTests`, `OrderEndpointsTests` (prato inativo e inexistente) |
| CA-06 | S: `OrderEndpointsTests` · I: `orderStatus.test.ts`, `OrderStatusPage.test.tsx` |
| CA-07 | S: `ComandaEndpointsTests` · E: `cliente.spec.ts` |
| CA-08 | S: `OrderServiceTests`, `ComandaEndpointsTests` · E: `cliente.spec.ts` |
| CA-09 | S: `AuthEndpointsTests` · E: `cozinha.spec.ts` |
| CA-10 | S: `AuthEndpointsTests` · E: `cozinha.spec.ts` |
| CA-11 | S: `AuthEndpointsTests` · E: `cozinha.spec.ts` |
| CA-12 ⚠️ | Sem checagem: comportamento a corrigir (cancelamento sem verificar o dono) |
| CA-13 | E: `cliente.spec.ts` (o nome cita CA-13); toda a suíte e2e roda no projeto `mock`, sem backend |

## Fora de escopo
- Pagamento, divisão de conta e emissão de nota.
- Cadastro de funcionários (existe um único login de cozinha).
- Gestão do cardápio e das mesas pela interface (só pela API, como Admin).

## Dúvidas em aberto
- O status `Draft` ("alteração solicitada") existe no backend mas o fluxo de alteração não está na interface. Mantém?
