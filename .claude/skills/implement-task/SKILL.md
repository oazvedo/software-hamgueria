---
name: implement-task
description: Implementa as tasks abertas de um docs/specs/NNN-nome/tasks.md em blocos (uma fase do tasks.md por vez), roda a verificação de cada task, a simplify e a verificação completa no fim do bloco, e marca como concluídas. Use quando o usuário pedir "implemente a próxima task", "continue a spec NNN", "faça a task X", ou quiser avançar a implementação de uma spec aprovada.
---

# Skill: implement-task

Trabalhe em **blocos**: um bloco é uma fase do `tasks.md` (ex.: "Fase 1: backend"). As tasks do bloco são feitas
em sequência, sem parar entre elas; a simplify, a verificação completa e o relatório acontecem uma vez, no fim do bloco.
Se o usuário pedir uma task específica, o bloco é só essa task.

## Passos
1. **Contexto.** Leia o `spec.md`, o `plan.md` e o `tasks.md` da spec, e também os ADRs citados no plan.
   Se a spec não estiver `Aprovada` ou `Em andamento`, avise o usuário e pergunte se ele quer seguir mesmo assim.
   Na primeira task, mude o status para `Em andamento` (spec e roadmap).
2. **Escolha o bloco.** A fase com a primeira task `- [ ]`, a não ser que o usuário tenha indicado outra.
   Anuncie em uma linha quais tasks vai fazer.
3. **Verifique a coerência.** Se uma task contradiz a spec, o plan ou um ADR, ou depende de algo que não existe,
   **pare** e explique. Não improvise. Decisão que muda o plan também é motivo para parar e perguntar.
4. **Para cada task do bloco, em ordem:**
   - Implemente só essa task. Siga `dotnet-feature` e `react-feature` quando se aplicarem: mudanças cirúrgicas,
     nada além do pedido. Dívida existente que você encontrar no caminho **não** se corrige agora (vai para o backlog).
   - Se houver suíte de testes, os testes da task fazem parte da task, com cenário de **sucesso** e de **falha**.
   - Rode a verificação da task (a do `tasks.md`; no mínimo `dotnet build backend/SoftwareHamgueria.slnx` ou
     `cd frontend && npm run build`). Se falhar, corrija e rode de novo. Nunca marque como concluída uma task com
     verificação vermelha.
   - Marque `- [x]` no `tasks.md` e siga para a próxima, sem relatório intermediário.
5. **Simplifique o bloco (OBRIGATÓRIO).** Rode a skill `simplify` **uma vez** sobre o diff do bloco inteiro.
   Nunca pule este passo nem decida que "não precisa". Aplique o que for do escopo do bloco.
   Achados que exigem mexer fora do bloco (outra entidade, outro ADR, refatoração geral) **não** viram trabalho agora:
   anote-os em `## Backlog` no fim do `tasks.md` (um item por achado, com o motivo) e siga.
6. **Verifique o bloco.** Depois da simplify, rode a verificação completa da camada tocada:
   - backend: `dotnet build backend/SoftwareHamgueria.slnx` (+ `dotnet test` quando existir suíte);
   - frontend: `cd frontend && npm run build && npm run lint` (+ `npm test` quando existir suíte), e os detectores
     Visant sobre os arquivos de UI tocados no bloco (`killer-scan`, `copy-lint`, `motion-lint`, comandos no
     `CLAUDE.md`, sempre com `npm_config_yes=false`). Achado nos arquivos do bloco é corrigido; fora deles, backlog.
7. **Reporte** ao usuário em até 10 linhas, uma vez por bloco:
   - as tasks feitas e os arquivos principais;
   - a saída resumida da verificação;
   - o resultado da `simplify` (aplicado e o que foi para o backlog);
   - **o que dá para aprender aqui**: o conceito central do bloco em 1 ou 2 frases;
   - qual é o próximo bloco.
8. **Pare** no fim do bloco. Não emende o próximo bloco sem o usuário pedir. Não commite sem o usuário pedir.

## Ao concluir a última task da spec
- Rode a skill `code-review` sobre o diff da spec inteira (e `security-review` se a spec mexeu em input, dados ou auth;
  `adversarial-review` se mexeu em pedidos, comandas ou auth; o fluxo completo da `visant-killer` se mudou telas,
  com relatório em `docs/audits/`).
- Confira se cada `CA-xx` do `spec.md` está coberto (teste que passa ou verificação manual registrada no plan).
- Mostre o `## Backlog` ao usuário: cada item vira uma task, uma spec nova (entra em "Candidatos a spec") ou é
  descartado, conforme ele decidir.
- Mude o status da spec para `Concluída` e atualize o roadmap.
- Sugira registrar o aprendizado com a skill `study-log`.
