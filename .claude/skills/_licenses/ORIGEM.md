# Skills de terceiros

## Visant (padrão de interface, ADR 0015)
Copiadas de [pedrojaques99/visant-skills-public](https://github.com/pedrojaques99/visant-skills-public) (MIT,
[LICENSE-visant](LICENSE-visant)), commit `fe555cfdeb405799db51ca52468079512c9adc1a` (2026-10-29), em 2026-10-31.
Conteúdo **sem modificações**: `visant-killer`, `visant-frontend`, `visant-motion`, `visant-refine`, `visant-copy`.

Auditoria dos scripts (2026-10-31): só leem o repo e gravam em pasta temporária ou no relatório pedido
(`--init-report`); os codemods da `visant-motion` só escrevem com `--apply`/`--fix`. Pontos de atenção:
- `killer-scan` executa `npx impeccable detect` **sem versão fixa** (baixaria do npm). Resolvido: `impeccable@4.8.0`
  (Apache-2.0, sem hooks de instalação) é devDependency exata do `frontend/`, e o `npm_config_yes=false` impede
  download quando falta. O launcher do `impeccable` só baixa do GitHub se o binário do npm faltar, e recusa
  download sem checksum. Binário `@impeccable/cli-linux-x64@0.1.18` conferido em 2026-10-31: SHA-256
  `736ed785…f648`, igual ao `engine-checksums.txt` do pacote.
- `killer-scan` roda scripts do próprio repo se existirem (`audit:design`, `lint:design`, `scripts/scan-design-*.mjs`).
- `verify-screens` faz `fetch` só para a `--base`/`--api` informada (localhost) e procura o Playwright no repo.
- Autotestes (`--self-test`) de `killer-scan`, `copy-lint`, `motion-lint` e `refine-scan` passaram com Node 24.

Não trazidas: `site-reels`, `visant-video-editor`, `visant-premiere-editor` (vídeo), `visant-methodology` (branding),
`visant-preflight` (projetos de marca), `visant-mcp-connect` (exige conta Visant Labs).

Removidas ao instalar a Visant: `better-interface` (orquestrador concorrente da `visant-killer`) e `interface-review`
(entrega toda revisão à `better-interface` e para sem ela; revisão de diff de UI agora é `visant-killer` +
`adversarial-review`). Para voltar com as duas, recopie do commit `645553c` do boraoztunc/skills.

## boraoztunc/skills

Copiadas de [boraoztunc/skills](https://github.com/boraoztunc/skills) (MIT), commit
`645553ca7622570479e330cc089c65fcf34e0ba8` (2026-08-15), em 2026-10-31. Conteúdo **sem modificações**.

| Skill | Origem | Licença |
|---|---|---|
| `better-accessibility`, `better-colors`, `better-layout`, `better-typography`, `better-ui`, `better-writing` (`better-interface` e `interface-review` removidas, ver acima) | [jakubkrehel/skills](https://github.com/jakubkrehel/skills) | MIT ([LICENSE-jakubkrehel](LICENSE-jakubkrehel), [NOTICE](NOTICE-jakubkrehel.md)) |
| `vercel-react-best-practices` | Vercel Engineering | MIT (frontmatter da skill) |
| `tailwind-v4`, `adversarial-review` | boraoztunc/skills | MIT (README do repositório) |

## Não trazidas, e por quê
- `web-design-guidelines`: busca as regras numa URL a cada uso (conteúdo remoto, não versionado); os `better-*` cobrem o mesmo assunto.
- `impeccable`, `apple-design`, `emil-design-eng`, `frontend-design`, `visual-style-presets`: skills de gosto/direção visual;
  o Hamgueria já tem identidade própria (escuro sóbrio + acento emerald, ADR 0011) e elas competiriam com os
  `better-*` pelos mesmos gatilhos.
- `linear-local-first-architecture`: propõe IndexedDB + motor de sync; desproporcional aqui. A parte útil
  (mutação otimista) é feita com o TanStack Query, se uma spec pedir.
- `conductor-rewrite-performance`: focada em app desktop Tauri com streaming; os `rerender-*` da Vercel cobrem o resto.

Ajustes de uso (conflitos com o projeto) estão no `CLAUDE.md`, seção "Precedência".
- Copywriting, SEO, analytics, vídeo (HyperFrames), shaders e efeitos: fora do escopo do projeto.

## Atualizar
```bash
git clone --depth 1 https://github.com/boraoztunc/skills /tmp/bora
for s in <skills da tabela>; do rm -rf ".claude/skills/$s" && cp -R "/tmp/bora/$s" ".claude/skills/$s"; done
```
Atualize o commit acima e revise o diff antes de commitar (é conteúdo que vira instrução para a IA).
