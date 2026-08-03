# 🍔 Software Hamgueria

O restaurante onde você monta seu pedido como se fosse um **JSON**.

Um projeto full-stack com temática _dev_ satírica: o cardápio é dividido em etapas de
um pipeline de CI/CD (**Setup · Build · Refactor · Runtime · Deploy**), o pedido é
construído ao vivo num painel estilo terminal, e a cozinha acompanha tudo como se fosse
um _deploy em produção_.

> Projeto de portfólio. Backend em .NET 10 (DDD) + frontend em React 19 (Feature-Sliced Design).

---

## ✨ Funcionalidades

- **Cardápio + pedido em JSON ao vivo** — monte o pedido e veja o payload se formar em
  tempo real num painel com cara de terminal.
- **Observações por item** — anote "sem cebola" e a cozinha recebe.
- **Comandas** — no checkout, um modal pergunta a comanda (seu nome). Uma mesa pode ter
  várias comandas; a mesma pessoa acumula vários pedidos na sua.
- **Acompanhamento em "pipeline de deploy"** — cada pedido passa por
  _Pedido recebido → Em produção → Servido_, atualizado ao vivo.
- **Visões do cliente** — sua comanda (`/comanda`) e a mesa inteira (`/mesa`).
- **Tela da cozinha** (`/producao`) — autenticada; a equipe vê todos os pedidos
  (por fila ou por mesa), finaliza, cancela e **fecha comandas**.
- **Fechar comanda** — cliente ou caixa encerram a comanda; ela sai das ativas e o mesmo
  nome pode abrir uma comanda nova depois.

## 🧱 Arquitetura

Monorepo com dois apps:

```
software-hamgueria/
├── backend/    .NET 10 · DDD (Domain / Application / Infrastructure / API)
└── frontend/   React 19 · Vite · Feature-Sliced Design + Atomic Design
```

**Backend (DDD, dependências sempre para dentro):**

- `Domain` — entidades (Dish, Table, Order, OrderItem, ComandaClosure) e enums.
- `Application` — DTOs, serviços e interfaces de repositório.
- `Infrastructure` — EF Core (Postgres), repositórios, migrations, RabbitMQ.
- `API` — controllers, auth JWT, seed inicial.

**Frontend (FSD):** `app → pages → widgets → features → entities → shared`.
Componentes visuais em `shared/ui` (átomos/moléculas + primitivos shadcn/ui).

## 🛠️ Stack

| | |
|---|---|
| **Backend** | .NET 10, EF Core + **Npgsql** (PostgreSQL), Migrations, JWT Bearer, RabbitMQ, Swagger |
| **Frontend** | React 19, TypeScript, Vite, Tailwind CSS v4, **shadcn/ui** (Radix), TanStack Query, Zustand |
| **Dev** | Docker Compose (Postgres + RabbitMQ), MSW (mocks de API no navegador) |

## 🚀 Rodando localmente

Pré-requisitos: **.NET 10 SDK**, **Node 20+**, **Docker**.

### 1. Backend

```bash
cd backend
docker compose up -d          # sobe Postgres + RabbitMQ
dotnet run --project src/SoftwareHamgueria.API
```

A API sobe em **http://localhost:5262**, aplica as migrations automaticamente e
semeia o banco (pratos + mesas). Swagger em `/swagger`.

- **Login da cozinha:** `admin` / `admin123`
- **RabbitMQ management:** http://localhost:15672 (`guest` / `guest`)

### 2. Frontend

```bash
cd frontend
npm install
npm run dev                   # http://localhost:5173
```

Por padrão o frontend usa **mocks (MSW)** e funciona **sem backend**. Para bater na API
real, crie um `.env` (veja `.env.example`):

```env
VITE_ENABLE_MOCKS=false       # usa o backend via proxy do Vite (→ :5262)
```

## 🗺️ Rotas do frontend

| Rota | Descrição |
|---|---|
| `/` | Home |
| `/menu` | Cardápio + montagem do pedido |
| `/order/:id` | Acompanhamento de um pedido |
| `/comanda/:tableId/:nome` | Comanda do cliente (todos os pedidos dela) |
| `/mesa/:tableId` | Todas as comandas de uma mesa |
| `/producao` | Tela da cozinha (autenticada) |

## 🗄️ Mudanças de schema (backend)

O schema é versionado com **EF Core Migrations** (aplicadas no startup via `Migrate()`):

```bash
cd backend
dotnet ef migrations add NomeDaMudanca \
  --project src/SoftwareHamgueria.Infrastructure \
  --startup-project src/SoftwareHamgueria.API
```

Depois é só reiniciar a API — a migration é aplicada preservando os dados.

---

> ⚠️ As credenciais em `appsettings.json` são apenas para desenvolvimento local.
