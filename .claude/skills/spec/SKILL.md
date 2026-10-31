---
name: spec
description: Cria a especificação de uma feature no fluxo spec-driven (spec.md, depois plan.md, depois tasks.md em docs/specs/NNN-nome/), com parada para revisão entre as fases. Use quando o usuário pedir uma feature nova, uma mudança de comportamento, "criar spec", "planejar feature", "quebrar em tasks", ou quando alguém pedir para implementar algo que ainda não tem spec.
---

# Skill: spec

O fluxo tem **3 fases**. Ao final de cada fase, **pare e peça revisão**. Só avance com a aprovação do usuário.
Se a spec já existir, retome a partir da primeira fase que ainda não foi aprovada.

## Fase 0: preparar
1. Leia `docs/specs/README.md` (roadmap, numeração e candidatos a spec) e `docs/adr/README.md`.
2. Leia a spec 001 (linha de base): ela descreve o comportamento que já existe. A spec nova não pode quebrar um
   `CA` da 001 sem dizer isso explicitamente.
3. Pasta: `docs/specs/NNN-nome-em-kebab-case/`, com o próximo número de 3 dígitos.
4. Se o pedido estiver vago, faça **no máximo 3 perguntas** objetivas antes de escrever.

## Fase 1: `spec.md` (o quê e por quê)
Use `docs/specs/_templates/spec.md`.
- **Não mencione tecnologia** (nada de controller, tabela, componente, hook). Descreva comportamento observável,
  com a linguagem do restaurante (cliente, comanda, mesa, cozinha, pedido, prato).
- Os critérios de aceite são numerados (`CA-01`...), no formato *Dado / Quando / Então*, e são testáveis.
  Inclua critérios de **falha** (entrada inválida, registro inexistente, prato inativo, sem permissão).
- A seção "Fora de escopo" é obrigatória.
- Status: `Rascunho`. ⏸️ **Pare** e peça revisão.

## Fase 2: `plan.md` (como)
Use `docs/specs/_templates/plan.md`.
- Liste os ADRs aplicados. **Siga os ADRs.** Se algo exigir uma decisão nova, use a skill `adr`
  (status `Proposto`) e cite o ADR novo.
- Siga as receitas das skills `dotnet-feature` e `react-feature` e liste os arquivos que serão criados ou alterados.
- Descreva o modelo de dados, os contratos da API (rotas, requests, responses, status, **público ou Admin**),
  o handler MSW correspondente (ADR 0013) e os riscos.
- Mapeie cada `CA-xx` para como ele será verificado (teste automatizado, se a base de testes existir; senão,
  verificação manual descrita passo a passo).
- ⏸️ **Pare** e peça revisão.

## Fase 3: `tasks.md` (passos)
Use `docs/specs/_templates/tasks.md`.
- Cada task: cerca de 30 minutos ou menos, **um** objetivo, os arquivos que toca e o comando de verificação.
- Ordem: as dependências primeiro (Domain → Infrastructure + migration → Application → API → mock MSW →
  entities → features → widgets → pages).
- Os testes entram junto com o código que eles cobrem, nunca em uma "task de testes" no final.
- Cada `CA-xx` aparece em pelo menos uma task.
- ⏸️ **Pare.** Com tudo aprovado, mude o status da spec para `Aprovada` e atualize o roadmap em `docs/specs/README.md`.

## Regras
- Não escreva código de produção nesta skill. Implementação é com a skill `implement-task`.
- Mantenha os documentos curtos. Se o `spec.md` passar de cerca de 2 páginas, provavelmente são duas specs.
- Se a spec resolve um item de "Candidatos a spec" no `docs/specs/README.md`, remova o item de lá ao aprovar.
