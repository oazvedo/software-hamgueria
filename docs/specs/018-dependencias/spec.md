# 018. Sem dependência com vulnerabilidade conhecida: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O backend herdava `System.Security.Cryptography.Pkcs` e `.Xml` 9.0.0 (alta) e os testes, `Newtonsoft.Json` 11.0.1
(alta); o frontend tinha 7 avisos altos (`axios`, `react-router`, `vite` e transitivas). Nada no CI impedia de voltar.

## Critérios de aceite
- **CA-01:** `dotnet list package --vulnerable --include-transitive` não acusa nada em nenhum projeto.
- **CA-02:** `npm audit --audit-level=high` não acusa nada.
- **CA-03 (falha):** Um PR que traga dependência vulnerável (alta ou crítica) falha no CI.

## Fora de escopo
- Avisos moderados e baixos (seguem visíveis no `npm audit`, sem bloquear).
