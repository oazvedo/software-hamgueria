---
name: implement-task
description: Implementa as tasks abertas de um docs/specs/NNN-nome/tasks.md de uma vez, sem parar entre fases, rodando a verificação de cada task e marcando como concluídas. Use quando o usuário pedir "implemente a próxima task", "continue a spec NNN", "faça a task X", ou quiser avançar a implementação de uma spec aprovada.
---

# Skill: implement-task

**Meta: rápido.** Uma spec inteira em até ~10 minutos. Faça todas as fases do `tasks.md` em sequência, sem parar entre
elas e sem relatório intermediário. Pare só no fim, ou se travar (ver passo 2).

## Passos
1. **Contexto.** Leia `spec.md`, `plan.md` e `tasks.md`. Mude o status para `Em andamento` (spec e roadmap).
2. **Pare só se** uma task contradiz a spec/plan/ADR, depende de algo que não existe ou exige uma ação do usuário
   (ex.: `sudo`). Explique em uma linha e pergunte. Fora isso, siga.
3. **Para cada task, em ordem:** implemente só ela (mudança cirúrgica; dívida achada no caminho vai para o
   `## Backlog`), rode a verificação da task, corrija se falhar, marque `- [x]`. Nunca marque task com verificação vermelha.
4. **No fim da spec, uma vez:**
   - suíte completa da camada tocada (`dotnet test --solution backend/SoftwareHamgueria.slnx` e/ou
     `cd frontend && npm test && npm run lint`);
   - uma passada rápida de limpeza **inline, sem subagentes**: duplicação óbvia, código morto, helper repetido. Não
     rode a skill `simplify` nem `code-review` a menos que o usuário peça;
   - se mudou tela: só o `killer-scan` + `copy-lint` nos arquivos tocados (`npm_config_yes=false`).
5. **Reporte em até 6 linhas:** tasks feitas, resultado da suíte, backlog novo, próximo passo. Status da spec →
   `Concluída` e roadmap atualizado. Não commite sem o usuário pedir.
