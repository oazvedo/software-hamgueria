# 022. As fronteiras do FSD verificadas pela máquina: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
A regra de imports do Feature-Sliced Design (ADR 0011) só era checada no olho; uma violação já tinha passado (spec 008)
e a verificação achou outras 4 escondidas.

## Critérios de aceite
- **CA-01:** Dado o frontend, quando o gate roda, então ele confere que cada camada só importa de camadas abaixo, que
  slices da mesma camada não se importam e que outro slice é importado pela API pública.
- **CA-02 (falha):** Dado um import que quebra qualquer das três regras, então o gate (e o CI) falha apontando arquivo e
  linha.
- **CA-03:** Dado o código atual, então o gate passa (as 4 violações achadas foram corrigidas).
