# CLAUDE.md

**Software Hamgueria**: restaurante temático dev. Backend .NET 10 (DDD) + frontend React 19 (FSD), construído
com IA seguindo um fluxo spec-driven (metodologia de `oazvedo/spec-driven-study`).
Documentação em PT-BR. **Código em inglês** (`Dish`, `Order`, `Table`), exceto o termo de domínio `comanda`;
textos de interface em português (ADR 0004).

## Antes de qualquer coisa
1. Leia os ADRs em `docs/adr/` (índice em `docs/adr/README.md`). **Eles são lei.** Se uma tarefa exigir
   contrariar um ADR, pare e proponha um novo ADR (skill `adr`). Não desvie em silêncio.
2. Trabalhe a partir de uma spec em `docs/specs/NNN-nome/`. Sem spec, crie uma (skill `spec`).
3. **Implementação rápida** (skill `implement-task`): a spec inteira de uma vez, meta de até ~10 minutos, sem parar
   entre fases. Cada task com a sua verificação; suíte completa e relatório curto só no fim. Pare só se travar.

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
sh scripts/ui/gate.sh [frontend/src/<alvo>]   # os 4 detectores de uma vez; sai 1 se algum achar algo (spec 006)
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
9. **Identidade dev vs. detector de ruído (decidido na spec 006, 2026-10-31):** ficam os rótulos de seção `// ...` e
   a nav `~/`, cada um com `EXCEÇÃO ao ruido-scan/code-crumb: <motivo>` na linha. Instrução nunca tem forma de
   comentário. Mono em tela de trabalho só em dado tabular, pelo átomo `DataText` (`shared/ui/atoms`). As bolinhas de
   janela são o átomo `WindowDots` (tokens `--window-*`). Código novo segue isso; o gate (`scripts/ui/gate.sh`) cobra.
10. **Relatórios** do `killer-scan` (`--init-report`/`--report`) ficam em `docs/audits/`, nunca na raiz.

## Skills de terceiros: referência (`.claude/skills/`, origem em `_licenses/ORIGEM.md`)
| Skill | Quando usar |
|---|---|
| `better-accessibility` | Valores exatos de a11y: alvo de toque, `focus-visible`, ARIA, `aria-live`, formulários |
| `better-writing` | Microcopy de UX: rótulo de botão, erro, vazio, confirmação (complementa a `visant-copy`) |
| `better-ui`, `better-layout`, `better-typography`, `better-colors` | Valores exatos quando a task mexe no domínio (sombras, raio, espaçamento, escala de tipo, contraste APCA) |
| `tailwind-v4` | Ao escrever utilities ou mexer em `@theme`/tokens de `app/styles/index.css` |
| `vercel-react-best-practices` | Ao escrever ou revisar componentes/hooks React com foco em performance |
| `adversarial-review` | Só quando o usuário pedir: revisão cética de um diff (pedidos, comandas, auth) |

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
- `simplify`, `code-review`, `security-review`, `adversarial-review`: **só quando o usuário pedir.** No fluxo normal,
  a limpeza é uma passada rápida inline no fim da spec (sem subagentes). Achados fora do escopo vão para o
  `## Backlog` do `tasks.md`.
- Se instalada: `andrej-karpathy-skills:karpathy-guidelines` (toda tarefa de código).
- `anthropic-skills:skill-creator`: para criar ou ajustar skills deste projeto.

## Regras rápidas (detalhes nos ADRs)
- **Backend:** DDD em 4 projetos (Domain, Application, Infrastructure, API); interfaces de repositório na
  Application; DI só em `ApplicationModule.cs` e `InfrastructureModule.cs` (ADR 0004). PostgreSQL + EF Core com uma
  `XxxConfiguration` por entidade, migrations aplicadas no startup + `DatabaseSeeder` (ADR 0005). Repositório
  persiste a própria escrita (ADR 0006). DTOs `record` com mapeamento manual (ADR 0007). Controllers em
  `/api/<entidade-singular>`, sem DELETE, Swagger em `/swagger` (ADR 0008). Entrada validada com FluentValidation no
  service (ADR 0021). Erros em ProblemDetails pelo handler global, sem `try/catch` no controller, com
  `NotFoundException`/`BusinessRuleException` (ADR 0022). JWT com papel `Admin` (ADR 0009). Eventos de pedido no RabbitMQ (ADR 0010), enfileirados em memória e enviados em segundo plano: pedido nunca falha por causa da fila (ADR 0025). Comanda = mesa + nome (ADR 0014).
  Segredos só por variável de ambiente, validados no startup fora de `Development` pelo `RequiredSettings` (ADR 0024).
- **Deploy:** uma EC2 com `docker-compose.prod.yml` (Postgres, RabbitMQ, API e Caddy servindo a tela e o `/api`), só
  80/443 abertas, segredos no `.env` da raiz (ADR 0023). Guia em `docs/deploy/aws-ec2.md`.
- **Frontend:** React + FSD + Atomic Design no `shared/ui` + shadcn/Tailwind (ADR 0011). TanStack Query para estado
  do servidor, Zustand para estado do cliente (ADR 0012). MSW ligado por padrão; todo endpoint novo tem handler (ADR 0013).
- **Testes (ADR 0016):** a verificação de toda task é o teste da parte tocada, não só o build. Servidor: xUnit v3 +
  NSubstitute (unidade) e WebApplicationFactory + Testcontainers (funcional, exige Docker). Interface: Vitest + Testing
  Library com os handlers MSW. Ponta a ponta: Playwright (`mock` sempre, `real` opcional). Nome do teste cita o
  critério: `"001/CA-04: pedido sem nome de comanda é recusado"`. Todo critério tem sucesso e, se houver, falha.

## Git (ADRs 0017 a 0020)
- Commits **sem** `Co-Authored-By` nem qualquer atribuição ao Claude.
- Só commite quando o usuário pedir.
- **`main`:** nunca commit nem push direto; só PR da `dev` (ou de `hotfix/*`), com os checks verdes.
- **`dev`:** commit e push direto são permitidos (sem force-push). Para uma spec grande ou para ter a revisão do
  Claude, use branch `feat/NNN-nome` e PR para a `dev` citando `docs/specs/NNN-nome`.
- Branch: `<tipo>/<descricao-em-kebab-case>`. Commits e títulos de PR em Conventional Commits
  (`tipo(escopo): descrição`); tipos `feat fix docs chore refactor test style perf ci build`. Não use `--no-verify`.
- Hooks versionados: rode uma vez por clone `git config core.hooksPath .githooks`. Regras em `scripts/git/convencao.sh`
  (testes: `sh scripts/git/convencao.test.sh`).
- O CI roda `backend`, `frontend` e `e2e` em todo push na `dev` e em PR; `convencao` e a revisão do Claude
  (`claude.yml`) rodam só em PR.
  Comente `@claude` no PR ou numa issue para pedir ajuste.

## Comandos
```bash
# Backend (Postgres na 5433 + RabbitMQ na 5673/15673 via Docker; valores em backend/.env, modelo .env.example)
cd backend && docker compose up -d
dotnet run --project backend/src/SoftwareHamgueria.API      # :5262, aplica migrations + seed; /swagger

# Testes do servidor (Microsoft.Testing.Platform via global.json: --solution/--project e --filter-class)
dotnet test --solution backend/SoftwareHamgueria.slnx                                        # tudo (~10 s, Docker ligado)
dotnet test --project backend/tests/SoftwareHamgueria.UnitTests --filter-class "*.OrderServiceTests"   # uma classe

# Migration nova
dotnet ef migrations add <Nome> -p backend/src/SoftwareHamgueria.Infrastructure -s backend/src/SoftwareHamgueria.API

# Frontend (:5173, MSW ligado por padrão; VITE_ENABLE_MOCKS=false usa a API real)
cd frontend && npm install && npm run dev
cd frontend && npm test                     # Vitest (~3 s); `npm test -- <arquivo>` filtra
cd frontend && npm run build && npm run lint
cd frontend && npm run e2e                  # Playwright com mocks (sobe o Vite na 5173)

# Ponta a ponta no modo real: API numa base separada, nunca a de dev (spec 002, RN-02). Precisa do RabbitMQ.
ConnectionStrings__Postgres="Host=localhost;Port=5433;Database=hamgueria_e2e;Username=hamgueria;Password=hamgueria123" \
  dotnet run --project backend/src/SoftwareHamgueria.API --launch-profile http
cd frontend && npm run e2e:real             # Vite na 5174 sem mocks; pula sozinho se a API não responder

# Produção (ADR 0023): fumaça local da pilha completa na 8088 (sobe, confere e derruba com volumes)
sh scripts/deploy/smoke.sh
docker compose -f docker-compose.prod.yml up -d --build    # no servidor, com o .env da raiz (docs/deploy/aws-ec2.md)
```
Playwright no WSL (uma vez): `npx playwright install chromium` e `sudo env "PATH=$PATH" npx playwright install-deps chromium`.
Login da cozinha (dev): `admin` / `admin123`.
