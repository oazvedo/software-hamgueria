# 013. CI: Plan

- **ADRs aplicados:** 0015, 0017, 0023, 0026. Sem ADR novo (o 0017 já define o CI; só ganha passos).

## Abordagem
`ci.yml`: passo "Gate de interface" no job `frontend`, depois do `npm ci` (o `impeccable` vem do `node_modules`);
job novo `images` com `docker/build-push-action@v6` (sem push) e cache `gha` por imagem.

## Verificação
| CA | Como |
|---|---|
| CA-01, CA-03 | O próprio PR: os checks `frontend` e `images` aparecem e passam |
| CA-02 | `sh scripts/ui/gate.test.sh` (amostras sujas reprovadas, spec 006) e o gate sai com erro, o que reprova o passo |
| CA-04 | `docker build` falha com Dockerfile inválido, o que reprova o passo |
