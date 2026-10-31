# CLAUDE.md

**Software Hamgueria**: restaurante temático dev. Backend .NET 10 (DDD) + frontend React 19 (FSD), construído
com IA seguindo um fluxo spec-driven (metodologia de `oazvedo/spec-driven-study`).
Documentação em PT-BR. **Código em inglês** (`Dish`, `Order`, `Table`), exceto o termo de domínio `comanda`;
textos de interface em português (ADR 0004).

## Antes de qualquer coisa
1. Leia os ADRs em `docs/adr/` (índice em `docs/adr/README.md`). **Eles são lei.** Se uma tarefa exigir
   contrariar um ADR, pare e proponha um novo ADR (skill `adr`). Não desvie em silêncio.
2. Trabalhe a partir de uma spec em `docs/specs/NNN-nome/`. Sem spec, crie uma (skill `spec`).
3. Implemente **um bloco por vez** (uma fase do `tasks.md`, skill `implement-task`): as tasks em sequência,
   cada uma com a sua verificação, e um relatório só no fim do bloco.

> **Projeto brownfield.** O código anterior à metodologia está descrito na spec 001 (linha de base) e nos ADRs
> retroativos. As dívidas conhecidas estão em `docs/specs/README.md` → "Candidatos a spec". Não corrija dívida
> fora do escopo da spec atual: anote no `## Backlog` do `tasks.md`.

## Skills do projeto (`.claude/skills/`)
| Skill | Quando usar |
|---|---|
| `spec` | Nova feature ou mudança de comportamento: cria spec.md → plan.md → tasks.md |
| `adr` | Qualquer decisão técnica nova ou mudança de uma decisão existente |
| `implement-task` | Executar o próximo bloco (fase) de tasks abertas de um `tasks.md` |
| `dotnet-feature` | Criar ou alterar entidade, endpoint, service, repositório ou migration no backend |
| `react-feature` | Criar ou alterar entity, feature, widget, page, API ou mock no frontend |
| `study-log` | Ao concluir um bloco ou spec: registrar o que foi aprendido |

## Interface: Visant é o padrão (ADR 0015)
Toda tela nova ou alterada passa pelo sistema Visant. Os `better-*` entram como referência de valores exatos.

| Skill | Papel |
|---|---|
| `visant-killer` | **Porta única de auditoria de UI.** Gate de detectores + julgamento + advogado do diabo + nota. Ao fim de todo bloco que mexeu em tela, e sempre que pedirem "audita essa tela" / "tá com cara de IA" |
| `visant-frontend` | **Decisão de interface:** cada elemento nomeia a variável de negócio que move (*cut test*), senão sai. Classificação de superfície (A aquisição, B trabalho, C confiança) antes de desenhar |
| `visant-motion` | Animação, transição, loading, skeleton: regras de easing/duração e `motion-lint` |
| `visant-copy` | Copy em pt-BR: proibições anti-tique (travessão, emoji, "não é X, é Y"...) e `copy-lint` |
| `visant-refine` | Gráficos, medidores, sliders, tiles de métrica (raro aqui) e `refine-scan` |

Comandos (rode da raiz do repo; **sempre** com `npm_config_yes=false`, veja ajuste 1):
```bash
export npm_config_yes=false
node .claude/skills/visant-killer/scripts/killer-scan.mjs frontend/src/<alvo>          # gate (3 de 4 detectores; copy roda à parte)
node .claude/skills/visant-copy/scripts/copy-lint.mjs frontend/src/<alvo>              # 4º detector, rodado à parte
node .claude/skills/visant-motion/scripts/motion-lint.mjs frontend/src/<alvo>
node .claude/skills/visant-killer/scripts/killer-scan.mjs --init-report docs/audits/AAAA-MM-DD-<alvo>.md frontend/src/<alvo>
node .claude/skills/visant-killer/scripts/killer-scan.mjs --report docs/audits/AAAA-MM-DD-<alvo>.md frontend/src/<alvo>
node .claude/skills/visant-killer/scripts/verify-screens.mjs --base http://localhost:5173 --login none   # exige Playwright
```

**Ajustes da Visant para este repo** (valem sobre o texto das skills):
1. **`impeccable`:** é devDependency do `frontend/` com versão exata (`4.8.0`, binário `@impeccable/cli-*@0.1.18`).
   Rode depois do `npm install` do frontend. O `npm_config_yes=false` continua obrigatório: sem ele, um clone sem
   `node_modules` faria o `npx` baixar a última versão do npm. Se o gate mostrar
   `[FAIL] impeccable ... npx canceled due to missing packages`, é **detector pulado** (falta `npm install`), não achado.
   Atualizar a versão é decisão consciente: revise o changelog e confira o checksum do binário em
   `node_modules/impeccable/skill/scripts/engine-checksums.txt`.
2. **Caminhos:** as skills citam `~/.claude/skills/...`; aqui é `.claude/skills/...`. O gate de copy do
   `killer-scan` procura o `copy-lint` só no `~` e aparece como `SKIPPED`: rode o `copy-lint` à parte (comando acima)
   e registre no relatório.
3. **Tokens:** a skill pede `hsl(var(--x))`; aqui os tokens já são cores `oklch` completas. Use as classes de token
   (`bg-card`, `text-muted-foreground`, `text-brand`) ou `var(--x)`. Cor crua (hex/rgb) é achado.
4. **Ícones:** a biblioteca é `lucide-react` (já instalada). Nada de glifo de seta (`→`) em texto nem SVG à mão;
   não adicione Phosphor/Radix Icons.
5. **Motion SSoT:** o repo ainda não tem `lib/motion.ts` nem `--dur-*`/`--ease-*`. Criar é decisão de ADR na primeira
   spec que precisar (siga o §10 da `visant-motion`). Até lá, siga as regras numéricas (UI < 300ms, nada de
   `transition-all`, só `transform`/`opacity`).
6. **Fora do repo:** a biblioteca `@visant/ui` (registry shadcn), o `revenue-centric-design`, `dataviz` e
   `vercel-react-view-transitions` não estão instalados. Adicionar registry ou skill é ADR; não instale por conta própria.
7. **"Gate vira fix, não proposta":** vale para achados **nos arquivos do bloco atual**. Achado fora do escopo vai
   para o `## Backlog` do `tasks.md` (mesma regra da `simplify`). Exceção a uma regra do detector só por escrito
   na linha (`EXCEÇÃO ao noise/<regra>: motivo`), nunca `audit-ignore`.
8. **Voz (decidido em 2026-10-31):** o glossário e a voz de marca da `visant-copy` são da Visant. Aqui a voz é a do
   Hamgueria (satírica dev), mas **as proibições valem para toda copy nova ou reescrita**: sem travessão, sem emoji,
   sem ponto-e-vírgula, sem "não é X, é Y". As 11 ocorrências antigas só mudam numa spec própria
   (ver "Candidatos a spec"); não as corrija de passagem.
9. **Identidade dev vs. detector de ruído (decidido em 2026-10-31):** o `noise-scan` marca `code-crumb`
   (`// monte seu pedido`, `~/menu`) e `mono-everywhere`, que são o conceito do produto (ADR 0011). **Não remova**
   esses motivos fora de uma spec que decida quais ficam (com exceção escrita na linha) e quais saem. Em blocos
   comuns, achados dessas duas regras em código já existente não são fix nem backlog: já estão em "Candidatos a spec".
10. **Relatórios** do `killer-scan` (`--init-report`/`--report`) ficam em `docs/audits/`, nunca na raiz.

## Skills de terceiros: referência (`.claude/skills/`, origem em `_licenses/ORIGEM.md`)
| Skill | Quando usar |
|---|---|
| `better-accessibility` | Valores exatos de a11y: alvo de toque, `focus-visible`, ARIA, `aria-live`, formulários |
| `better-writing` | Microcopy de UX: rótulo de botão, erro, vazio, confirmação (complementa a `visant-copy`) |
| `better-ui`, `better-layout`, `better-typography`, `better-colors` | Valores exatos quando a task mexe no domínio (sombras, raio, espaçamento, escala de tipo, contraste APCA) |
| `tailwind-v4` | Ao escrever utilities ou mexer em `@theme`/tokens de `app/styles/index.css` |
| `vercel-react-best-practices` | Ao escrever ou revisar componentes/hooks React com foco em performance |
| `adversarial-review` | Ao concluir uma spec que mexe em pedidos, comandas ou auth, junto com o `code-review` |

**Precedência:** ADRs > `CLAUDE.md` > skills do projeto > Visant > demais skills de terceiros. Em conflito entre
Visant e `better-*` sobre **o que** fazer, vale a Visant; sobre **o valor exato** (px, contraste, ms), vale o `better-*`.
Ajustes conhecidos:
- `vercel-react-best-practices` foi escrita para Next.js (aqui é SPA com Vite):
  - ignore `server-*`, `async-api-routes`, `rendering-hydration-no-flicker` e tudo de RSC;
  - `bundle-dynamic-imports`: troque `next/dynamic` por `React.lazy` + `Suspense`;
  - `bundle-barrel-imports` vale só para bibliotecas de terceiros e **não** derruba os `index.ts` das slices do FSD
    (ADR 0011); não troque os imports do `lucide-react` por caminhos `dist/esm/...` (o Vite já faz tree-shaking);
  - `client-swr-dedup` vale como princípio, mas a ferramenta é TanStack Query (ADR 0012).
- `tailwind-v4` manda declarar tokens direto em `@theme`; aqui o padrão é o do shadcn: variável em `:root` **e** em
  `.dark`, mapeada em `@theme inline` (`app/styles/index.css`). Ela cita `visual-style-presets` e
  `beautiful-shadows`, que não estão instaladas: ignore.
- `adversarial-review` sugere criar branch e **commitar** antes de revisar: não faça. Revise por `git diff` (working
  tree ou `main...HEAD`). Use o modo inline; só abra agentes em paralelo se o usuário pedir.
- `better-writing` tem exemplos em inglês: aplique os princípios em português. O tom satírico vale para cardápio,
  vazio e sucesso; **erros e confirmações destrutivas** (cancelar pedido, fechar comanda) são sérios e diretos.
  Em conflito com a `visant-copy` (pontuação, emoji, tiques), vale a `visant-copy`.
- Os `better-*` sugerem valores; tokens novos de cor/tipo entram em `app/styles/index.css`, não inline.

## Skills globais e quando usar
- `simplify`: **obrigatória** uma vez por bloco (sobre o diff do bloco inteiro), antes do relatório e de commitar.
  Nunca pule, nem em blocos pequenos. Achados fora do escopo do bloco vão para o `## Backlog` do `tasks.md`.
- `code-review`: ao terminar todas as tasks de uma spec.
- `security-review`: ao mexer em input do usuário, acesso a dados, autenticação (JWT/admin) ou permissões.
- Se instalada: `andrej-karpathy-skills:karpathy-guidelines` (toda tarefa de código).
- `anthropic-skills:skill-creator`: para criar ou ajustar skills deste projeto.

## Regras rápidas (detalhes nos ADRs)
- **Backend:** DDD em 4 projetos (Domain, Application, Infrastructure, API); interfaces de repositório na
  Application; DI só em `ApplicationModule.cs` e `InfrastructureModule.cs` (ADR 0004). PostgreSQL + EF Core com uma
  `XxxConfiguration` por entidade, migrations aplicadas no startup + `DatabaseSeeder` (ADR 0005). Repositório
  persiste a própria escrita (ADR 0006). DTOs `record` com mapeamento manual (ADR 0007). Controllers em
  `/api/<entidade-singular>`, sem DELETE, Swagger em `/swagger` (ADR 0008). JWT com papel `Admin` (ADR 0009).
  Eventos de pedido no RabbitMQ (ADR 0010). Comanda = mesa + nome (ADR 0014).
- **Frontend:** React + FSD + Atomic Design no `shared/ui` + shadcn/Tailwind (ADR 0011). TanStack Query para estado
  do servidor, Zustand para estado do cliente (ADR 0012). MSW ligado por padrão; todo endpoint novo tem handler (ADR 0013).
- **Testes:** ainda não há suíte (veja "Candidatos a spec"). Até existir, a verificação mínima de uma task é o build
  e o lint da camada (abaixo). Quando a spec de testes for aceita, ela vira a regra.

## Git
- Commits **sem** `Co-Authored-By` nem qualquer atribuição ao Claude.
- Só commite quando o usuário pedir.

## Comandos
```bash
# Backend (Postgres + RabbitMQ via Docker)
cd backend && docker compose up -d
dotnet run --project backend/src/SoftwareHamgueria.API      # :5262, aplica migrations + seed; /swagger
dotnet build backend/SoftwareHamgueria.slnx                 # verificação mínima do backend

# Migration nova
dotnet ef migrations add <Nome> -p backend/src/SoftwareHamgueria.Infrastructure -s backend/src/SoftwareHamgueria.API

# Frontend (:5173, MSW ligado por padrão; VITE_ENABLE_MOCKS=false usa a API real)
cd frontend && npm install && npm run dev
cd frontend && npm run build && npm run lint                # verificação mínima do frontend
```
Login da cozinha (dev): `admin` / `admin123`.
