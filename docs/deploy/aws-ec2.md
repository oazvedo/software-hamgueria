# Deploy na AWS (EC2 + Docker Compose)

Como publicar o Hamgueria numa instância EC2 (ADR 0023), com os segredos no `.env` (ADR 0024). O provisionamento é
manual. Uma instância roda tudo: Postgres, RabbitMQ, a API e o Caddy, que serve a tela, repassa o `/api` e cuida do
HTTPS.

```
internet ──80/443──▶ web (Caddy) ──/api──▶ api:8080 ──▶ postgres:5432
                        └── tela (SPA)          └─────▶ rabbitmq:5672
```

## 1. Criar a instância
1. **EC2 → Launch instance.** Imagem: Ubuntu Server 24.04 LTS (x86_64). Tipo: `t3.small` (2 GB). Com 1 GB
   (`t3.micro`), o build da API pode faltar memória.
2. **Disco:** 20 GB gp3.
3. **Key pair:** crie ou escolha uma chave SSH.
4. **Security group** (entrada):

   | Porta | Origem | Para quê |
   |---|---|---|
   | 22 | só o seu IP (`x.x.x.x/32`) | SSH |
   | 80 | `0.0.0.0/0` | HTTP (e validação do certificado) |
   | 443 | `0.0.0.0/0` | HTTPS |

   Não abra 5432, 5672 nem 15672: banco e fila não publicam portas, e o security group é a segunda barreira.
5. **Elastic IP:** aloque e associe à instância, para o endereço não mudar quando ela reiniciar.

## 2. Preparar o servidor
```bash
ssh -i sua-chave.pem ubuntu@<elastic-ip>

# Docker Engine + plugin compose (repositório oficial do Docker)
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker ubuntu && exit     # saia e entre de novo para valer o grupo
```

## 3. Clonar e configurar
```bash
git clone https://github.com/oazvedo/software-hamgueria.git hamgueria && cd hamgueria
git switch main

cp .env.example .env && chmod 600 .env
nano .env
```
Preencha cada item seguindo os comentários do `.env.example`. Para gerar os segredos:
```bash
openssl rand -hex 24   # POSTGRES_PASSWORD, RABBITMQ_PASSWORD
openssl rand -hex 32   # AUTH_JWT_SECRET
```
A senha da cozinha (`AUTH_ADMIN_PASSWORD`) precisa de pelo menos 12 caracteres e não pode ser `admin123`. A chave
do login precisa de pelo menos 32. Se faltar algo, o compose recusa subir e diz qual variável falta. Se algo for
fraco, a API não inicia e o log diz qual regra falhou.

**Não troque `POSTGRES_PASSWORD` depois do primeiro deploy:** o Postgres só lê essa variável ao criar o banco.
Para trocar, altere a senha dentro do banco (`ALTER USER hamgueria PASSWORD '...'`) e depois o `.env`.

## 4. Domínio e HTTPS
- **Com domínio:** crie um registro `A` apontando para o Elastic IP e ponha o domínio em `SITE_ADDRESS`
  (ex.: `SITE_ADDRESS=hamgueria.exemplo.com.br`). O Caddy obtém e renova o certificado sozinho. As portas 80 e 443
  precisam estar abertas.
- **Sem domínio:** deixe `SITE_ADDRESS=:80`. Funciona pelo IP, **sem conexão segura**: a senha da cozinha trafega
  aberta. Use só para testar.

## 5. Subir
```bash
docker compose -f docker-compose.prod.yml up -d --build --wait
docker compose -f docker-compose.prod.yml ps
curl -s http://localhost/api/health        # Healthy
```
Na primeira subida, a API aplica as migrations e semeia cardápio e mesas (ADR 0005). Abra `http://<elastic-ip>`
(ou `https://<domínio>`) e entre na cozinha em `/producao` com o usuário e a senha do `.env`.

## 6. Publicar uma versão nova
```bash
cd ~/hamgueria && git pull
docker compose -f docker-compose.prod.yml up -d --build --wait
```
Os dados ficam nos volumes (`hamgueria_postgres_data` e os demais) e sobrevivem a rebuild e reinício. **Nunca** use
`down -v` em produção: o `-v` apaga os volumes.

Depois de reiniciar a instância ou rodar `restart`, rode `up -d --wait` de novo. O `restart` ignora a ordem de
dependência, e a API pode voltar antes da fila. Enquanto isso, as rotas de pedido respondem erro (ver Backlog da
spec 004).

## 7. Cópia de segurança (manual)
Não há backup automático. Faça antes de cada deploy e com frequência:
```bash
docker compose -f docker-compose.prod.yml exec -T postgres pg_dump -U hamgueria -Fc hamgueria > hamgueria-$(date +%F).dump
# restaurar:
docker compose -f docker-compose.prod.yml exec -T postgres pg_restore -U hamgueria -d hamgueria --clean < hamgueria-AAAA-MM-DD.dump
```
Guarde o arquivo fora da instância (ex.: S3). Um snapshot do volume EBS também serve.

## 8. Logs e diagnóstico
```bash
docker compose -f docker-compose.prod.yml logs -f api        # ou web, postgres, rabbitmq
docker compose -f docker-compose.prod.yml ps                 # estado e saúde
```
Os logs giram em 3 arquivos de 10 MB por serviço. Um erro 500 mostra um `traceId` ao cliente. Procure por ele no log
da `api` (ADR 0022).

## Antes do primeiro deploy, teste local
Da raiz do repositório, `sh scripts/deploy/smoke.sh` sobe a mesma pilha na porta 8088 com segredos descartáveis,
confere tela, API, saúde, portas fechadas, `/swagger` 404 e persistência após reinício, e derruba tudo.
