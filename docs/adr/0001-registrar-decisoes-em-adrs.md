# 0001. Registrar decisões em ADRs

- **Status:** Proposto
- **Data:** 2026-10-31

## Contexto
As decisões do Hamgueria estavam espalhadas entre o README, `backend/backend-requirements.md`,
`frontend/frontend-requirements.md` e comentários no código. Sem um registro único, a IA (e as pessoas) não sabem
o que é regra e o que foi acidente, e acabam desviando em silêncio.

## Decisão
Toda decisão técnica fica em `docs/adr/NNNN-titulo.md`, no formato do [template](template.md), com índice em
`docs/adr/README.md`. Um ADR aceito não se edita: para mudar a decisão, cria-se outro que o substitui.
Decisões que já existiam no código são registradas como ADRs retroativos, descrevendo o estado atual.

## Alternativas consideradas
- **Manter as decisões no README:** o README é para quem usa o projeto, não guarda o porquê nem as alternativas.
- **Wiki externa:** fica fora do repositório, então a IA não lê e o histórico não acompanha o código.

## Consequências
- ✅ A IA tem regras explícitas para seguir, e cada desvio vira uma discussão visível.
- ✅ O "porquê" de cada escolha fica registrado para estudo.
- ⚠️ Mais um artefato para manter; ADRs retroativos podem registrar decisões que hoje não seriam tomadas.
