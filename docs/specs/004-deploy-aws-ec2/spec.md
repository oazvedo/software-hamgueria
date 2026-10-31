# 004. Preparar o Hamgueria para rodar num servidor na nuvem: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
Hoje o Hamgueria só roda na máquina de quem desenvolve. Não existe um jeito documentado de colocá-lo num servidor
público para clientes e cozinha usarem de verdade. Pior: a senha da cozinha, a chave que assina o login e a senha do
banco estão gravadas no repositório, e o sistema tem uma senha de cozinha padrão embutida que vale se ninguém configurar
outra. Qualquer pessoa que leia o código entraria na cozinha de um restaurante publicado assim. Além disso, o banco e a
fila de mensagens usam as portas padrão e colidem com outros projetos na mesma máquina (já aconteceu na spec 002).
Quem sente: o **mantenedor**, que não consegue publicar com segurança, e a **cozinha**, cujo acesso fica exposto.

## Objetivo
O mantenedor publica o Hamgueria num servidor novo seguindo um guia, preenchendo um único arquivo de configuração que
fica fora do repositório. Em produção, o sistema se recusa a subir sem segredos próprios e fortes, e só a porta web
fica aberta para a internet. Na máquina de desenvolvimento, banco e fila usam portas que não colidem com outros projetos.

## Histórias de usuário
- Como **mantenedor**, quero publicar o restaurante num servidor novo seguindo passos escritos, sem adivinhar nada.
- Como **mantenedor**, quero que o sistema se recuse a subir em produção com senha padrão ou sem segredo, para eu não
  publicar uma cozinha aberta por descuido.
- Como **cozinha**, quero que só quem tem a senha definida pelo restaurante consiga entrar.
- Como **dev**, quero subir banco e fila localmente sem brigar com outro projeto pela mesma porta.

## Regras de negócio
- RN-01 (segredos): nenhum segredo de produção fica no repositório. A senha da cozinha, a chave do login e as senhas do
  banco e da fila vêm de um arquivo de configuração local, que o repositório ignora. Um modelo desse arquivo, sem
  valores reais, fica versionado e explica cada item.
- RN-02 (sem padrão embutido): o sistema não tem mais senha de cozinha embutida no código. Em produção, faltando
  qualquer segredo obrigatório, ele não sobe e diz **qual** item falta, sem mostrar valor nenhum.
- RN-03 (segredo forte): em produção, a chave do login tem pelo menos 32 caracteres e a senha da cozinha não pode ser a
  senha de desenvolvimento (`admin123`) nem ter menos de 12 caracteres. Fora disso, o sistema não sobe.
- RN-04 (portas locais): no desenvolvimento, o banco atende na porta **5433**, a fila na **5673** e o painel da fila na
  **15673**, para não colidir com instalações padrão. A API continua na 5262 e a tela na 5173.
- RN-05 (superfície pública): no servidor, só a porta web fica aberta. Banco, fila e painel da fila não respondem a
  quem está fora do servidor. A documentação interativa da API não fica disponível em produção (como hoje).
- RN-06 (um endereço só): em produção, a tela e a API respondem no mesmo endereço público. A tela usa sempre os dados
  reais, nunca os simulados.
- RN-07 (conexão segura): com um domínio configurado, o acesso é por conexão segura (HTTPS), com certificado obtido
  sozinho. Sem domínio, funciona pelo endereço IP sem conexão segura, e o guia avisa que isso expõe a senha da cozinha.
- RN-08 (dados persistem): reiniciar o servidor ou publicar uma versão nova não apaga cardápio, mesas, pedidos nem comandas.
- RN-09 (saúde): existe um endereço de verificação que responde "saudável" quando o sistema consegue falar com o banco,
  e "indisponível" quando não consegue. Ele não exige login e não revela detalhes internos.
- RN-10 (desenvolvimento intacto): o desenvolvimento local continua funcionando com o login `admin` / `admin123` e com
  os dados simulados ligados por padrão (ADR 0013).

## Critérios de aceite
- **CA-01:** Dado um clone novo com o arquivo de configuração copiado do modelo, quando o dev sobe banco e fila e roda a
  API, então eles atendem nas portas 5433, 5673 e 15673, a API conecta e o fluxo da spec 001 funciona.
- **CA-02:** Dado outro banco já ocupando a porta padrão na máquina, quando o dev sobe o ambiente do Hamgueria, então os
  dois convivem sem conflito.
- **CA-03:** Dado um servidor com o arquivo de configuração preenchido, quando o mantenedor sobe o ambiente de produção
  seguindo o guia, então o cardápio real abre no endereço público e a cozinha entra com a senha definida no arquivo.
- **CA-04 (falha):** Dado o ambiente de produção sem a senha da cozinha (ou sem a chave do login, ou sem a senha do
  banco), quando ele sobe, então a API não inicia e o registro diz qual item falta, sem exibir nenhum valor.
- **CA-05 (falha):** Dado o ambiente de produção com a senha `admin123`, com uma senha de 11 caracteres ou com uma chave
  de login de 31 caracteres, quando ele sobe, então a API não inicia e diz qual regra foi violada. Com 12 e 32
  caracteres, sobe.
- **CA-06 (falha):** Dado o ambiente de produção no ar, quando alguém de fora do servidor tenta conectar no banco, na
  fila ou no painel da fila, então não há resposta. Quando pede a documentação interativa da API, recebe "não encontrado".
- **CA-07:** Dado o ambiente de produção com pedidos e comandas, quando o mantenedor reinicia tudo ou publica uma versão
  nova, então os dados continuam lá.
- **CA-08:** Dado o ambiente de produção, quando o cliente monta e envia um pedido pela tela publicada, então o pedido
  chega na cozinha real (nada vem dos dados simulados).
- **CA-09:** Dado o sistema no ar, quando alguém consulta o endereço de saúde, então recebe "saudável". **(falha)** Com o
  banco parado, recebe "indisponível", sem detalhes internos.
- **CA-10 (falha):** Dado o repositório, quando alguém procura segredos de produção ou o arquivo de configuração
  preenchido, então não encontra: o arquivo é ignorado e o código não tem senha da cozinha embutida.

## Impacto na linha de base
Nenhum `CA` da 001 muda. Mudam os endereços locais do banco e da fila (documentação e comandos do `CLAUDE.md` e do
`README.md`), e a senha de desenvolvimento deixa de ser um valor embutido no código para vir da configuração de
desenvolvimento.

## Fora de escopo
- Criar a infraestrutura na nuvem por código (servidor, rede, regras de firewall) e a publicação automática a cada
  commit. Nesta spec, o provisionamento é manual, descrito no guia.
- Serviços gerenciados de banco e de fila, múltiplos servidores, balanceamento, escala.
- Cópia de segurança automática do banco e monitoramento. O guia cita o risco e um comando manual de cópia.
- Vários usuários na cozinha, troca de senha pela tela e rotação automática de segredos (ADR 0009 continua).
- Compra e configuração de domínio. O guia só diz onde informar o domínio.

## Dúvidas em aberto
- Já existe um domínio para o restaurante? Sem ele, o primeiro deploy fica sem conexão segura (RN-07).
- Região e tamanho do servidor: a recomendação do guia será uma instância pequena (2 GB de memória), revisável.
