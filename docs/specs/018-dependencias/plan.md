# 018. Plan

- **ADRs aplicados:** 0017 (CI). Sem ADR novo.

## Abordagem
Referência direta às versões corrigidas: `System.Security.Cryptography.Pkcs`/`.Xml` 10.0.14 (Infrastructure) e
`Newtonsoft.Json` 13.0.4 (testes funcionais). `npm audit fix` sem `--force` (só o lockfile mudou). CI: passo que falha se
o `dotnet list ... --vulnerable` acusar algo, e `npm audit --audit-level=high` no job `frontend`.

## Risco
Um aviso novo publicado no npm ou no NuGet pode reprovar um PR que não mexeu em dependência. É o comportamento
desejado: o PR seguinte corrige a dependência.

Evidência do CA-03 (2026-10-31): com `Newtonsoft.Json` 11.0.1 acrescentado de propósito aos testes de unidade, `sh scripts/ci/nuget-vulnerabilities.sh` sai com 1 ("pacote vulnerável"); sem ele, sai 0 com 6 projetos confirmados.
