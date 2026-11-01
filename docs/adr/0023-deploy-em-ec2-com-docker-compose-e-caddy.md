# 0023. Deploy em uma instância EC2 com Docker Compose e Caddy

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
O Hamgueria só roda na máquina de desenvolvimento (`backend/docker-compose.yml` sobe Postgres e RabbitMQ, a API roda
com `dotnet run` e a tela com o Vite). Não há imagem da API nem da tela, nem um jeito documentado de publicar. É um
projeto de estudo com tráfego pequeno: o custo e o número de peças importam mais que alta disponibilidade. A spec 004
pede a publicação na AWS com provisionamento manual, sem infraestrutura como código.

## Decisão
- **Uma instância EC2** (Amazon Linux 2023 ou Ubuntu, sugestão `t3.small`, 2 GB) roda tudo com
  `docker compose -f docker-compose.prod.yml` na raiz do repositório. As imagens são construídas na própria instância
  (`--build`), a partir do clone do repositório. Sem ECR nem pipeline de deploy nesta fase.
- **Serviços:** `postgres` (16-alpine), `rabbitmq` (3.13-alpine, sem plugin de management), `api` e `web`.
  Só o `web` publica portas (80 e 443). Banco e fila ficam só na rede interna do compose, com volumes nomeados.
- **`backend/Dockerfile`:** multi-stage (`dotnet/sdk:10.0` → `dotnet/aspnet:10.0`), usuário não-root `app`,
  porta 8080, `ASPNETCORE_ENVIRONMENT=Production`.
- **`frontend/Dockerfile`:** multi-stage (`node:24-alpine` com `npm ci` e `VITE_ENABLE_MOCKS=false` → `caddy:2-alpine`).
  O **Caddy** serve o `dist/` com fallback de SPA para `index.html`, repassa `/api/*` para `api:8080` e emite o
  certificado HTTPS sozinho quando `SITE_ADDRESS` é um domínio. Com `SITE_ADDRESS=:80`, serve HTTP pelo IP.
- **Saúde:** a API expõe `GET /api/health` (público, `Healthy`/`Unhealthy`, 200/503) com checagem do banco. O compose
  usa esse endereço no `healthcheck` da `api`, e o `web` só sobe com a `api` saudável.
- **Security group:** entrada só em 22 (do IP do mantenedor), 80 e 443. O passo a passo fica em `docs/deploy/aws-ec2.md`.

```yaml
# docker-compose.prod.yml (trecho)
web:
  build: ./frontend
  ports: ["${HTTP_PORT:-80}:80", "${HTTPS_PORT:-443}:443"]
  environment: { SITE_ADDRESS: "${SITE_ADDRESS:-:80}" }
postgres:
  image: postgres:16-alpine   # sem "ports": só a rede interna enxerga
```

## Alternativas consideradas
- **ECS Fargate + RDS + Amazon MQ + S3/CloudFront:** pronto para escala, mas custa várias vezes mais por mês e
  exige IAM, VPC, task definitions e IaC para ser mantível. Excessivo para o tráfego do projeto.
- **App Runner + RDS + Amazon MQ:** menos peças que o ECS, mas mantém o custo dos serviços gerenciados e separa a tela
  da API em dois endereços (CORS e configuração de URL no build).
- **Nginx no lugar do Caddy:** igualmente capaz de servir a SPA e fazer proxy, mas HTTPS exige certbot e renovação
  agendada. O Caddy faz isso sozinho com uma linha.

## Consequências
- ✅ Custo de uma instância pequena, e o ambiente de produção é quase o mesmo que roda localmente com compose.
- ✅ Tela e API no mesmo endereço: sem CORS e sem `VITE_API_BASE_URL` no build.
- ⚠️ Ponto único de falha: se a instância cair, o restaurante cai. Sem cópia de segurança automática do banco
  (o guia traz um `pg_dump` manual).
- ⚠️ Build na instância consome memória e CPU durante o deploy; numa `t3.micro` (1 GB) o build da API pode falhar.
- ⚠️ Deploy manual (`git pull` + `docker compose up -d --build`). Pipeline e IaC ficam para uma spec futura.
