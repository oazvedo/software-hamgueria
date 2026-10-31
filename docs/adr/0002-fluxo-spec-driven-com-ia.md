# 0002. Fluxo spec-driven com IA

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
O Hamgueria cresceu com pedidos diretos à IA ("adiciona comandas", "tela da cozinha"). Funcionou, mas gerou diffs
grandes, sem critérios de aceite e sem testes. Para seguir evoluindo o projeto e aprender com cada passo, cada mudança
precisa ser pequena, revisável e verificável.

## Decisão
Toda feature passa por três documentos em `docs/specs/NNN-nome/`, revisados nesta ordem:
1. **`spec.md`**: o quê e por quê. Problema, histórias de usuário, critérios de aceite testáveis
   e o que fica fora de escopo. **Não menciona tecnologia.**
2. **`plan.md`**: como. Abordagem técnica, ADRs aplicados, modelo de dados, contratos da API e riscos.
   Se exigir uma decisão nova, cria um ADR `Proposto`.
3. **`tasks.md`**: checklist de tasks de até ~30 minutos, cada uma com o comando que a verifica.

A IA implementa **um bloco (fase) por vez** com a skill `implement-task`: roda a verificação de cada task, marca `[x]`,
roda a `simplify` sobre o bloco e para para revisão.

## Alternativas consideradas
- **Um único `spec.md` por feature:** mais leve, mas mistura o quê com o como e gera tasks grandes demais.
- **Sem specs, só prompts:** foi o que se fez até aqui; rápido, mas não deixa histórico nem critérios de aceite.

## Consequências
- ✅ Diffs pequenos e revisáveis, e cada um ensina algo.
- ✅ Critérios de aceite viram testes (quando a base de testes existir).
- ⚠️ Mais arquivos e etapas para features triviais (nesses casos, as tasks podem ser poucas).
