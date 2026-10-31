# 018. Plan

- **ADRs aplicados:** 0017 (CI). Sem ADR novo.

## Abordagem
Referência direta às versões corrigidas: `System.Security.Cryptography.Pkcs`/`.Xml` 10.0.14 (Infrastructure) e
`Newtonsoft.Json` 13.0.4 (testes funcionais). `npm audit fix` sem `--force` (só o lockfile mudou). CI: passo que falha se
o `dotnet list ... --vulnerable` acusar algo, e `npm audit --audit-level=high` no job `frontend`.

## Risco
Um aviso novo publicado no npm ou no NuGet pode reprovar um PR que não mexeu em dependência. É o comportamento
desejado: o PR seguinte corrige a dependência.
